using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using BrewduinoCatalogLib;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.IO.Ports;
using System.Threading;

namespace SerialPortSwitchService
{


    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    class SerialSwitchServiceHost : IArduinoSelfHost
    {
        private static Dictionary<string, float> _Status;
        Dictionary<string, float> Status
        {
            get { return _Status; }
            set { _Status = value; }
        }
        private static SerialPort _Serial;
        private static int count;
        public SerialSwitchServiceHost()
        {
            _Status = new Dictionary<string, float>();
        }

        private void setPort(SerialPort port)
        {
            if (count == null)
            {
                Console.WriteLine("Initializing Count");
                count = 0;
            }
            _Serial = port;
        }

        public string GetRawStatus()
        {
            return string.Empty;
        }

        public Dictionary<string, float> GetStatus()
        {
            count = count + 1;
            return Status;
        }

        public void SendCommand(int arduinoCommands, string text)
        {
            StringBuilder sendCmd = new StringBuilder();

            sendCmd.Append(arduinoCommands);
            if (!string.IsNullOrEmpty(text))
                sendCmd.Append("," + text + ";");
            else
                sendCmd.Append(";");


            if (sendCmd != null && !String.IsNullOrEmpty(sendCmd.ToString()))
            {
                _Serial.WriteLine(sendCmd.ToString());
            }
        }


        //public Dictionary<string, float> SendCommandWithResponse(ArduinoCommands.CommandTypes cmd, string text)
        //{

        //    SendCommand(cmd, text);

        //    return Status;
        //}

        public void UpdateStatus()
        {
            //SendCommand(ArduinoCommands.CommandTypes.ReturnStatus, "");
        }

        public void readSerial()
        {
            StringBuilder response = new StringBuilder();

            while (true)
            {
                try
                {
                    //System.Threading.Thread.Sleep(5000);
                    response = new StringBuilder();
                    response.Append(_Serial.ReadTo(";"));
                    response = response.Replace("\r", "");
                    response = response.Replace("\n", "");
                    response = response.Replace(" ", "");

                }
                catch
                {
                    //Close();
                    //return string.Empty;
                }

                Console.WriteLine(response.ToString());
                Dictionary<string, float> responseDictionary = parseVaribles(response.ToString());

                foreach (var item in responseDictionary)
                {
                    _Status[item.Key] = item.Value;
                }



                Console.WriteLine("We now have " + _Status.Count + " Items in the status. Count = " + count);
            }

        }
        public Dictionary<string, float> parseVaribles(string response)
        {
            string[] pStrings;
            string message;
            if (response.Contains(':'))
            {
                pStrings = response.Split(':');
                message = pStrings[1];
            }
            else
            {
                message = response;
            }

            if (message.Contains(';'))
            {
                pStrings = message.Split(';');
                message = pStrings[0];
            }

            Dictionary<string, float> dict = new Dictionary<string, float>();

            string[] pairs = message.Split(',');

            foreach (string textValue in pairs)
            {
                string[] pair = textValue.Split('|');

                if (pair.Length != 2)
                    break;

                float temp;
                float.TryParse(pair[1], out temp);

                dict.Add(pair[0], temp);
            }


            return dict;
        }

        public void OpenPort()
        {
            if (!_Serial.IsOpen)
                _Serial.Open();

            string throwaway = _Serial.ReadExisting();

        }
        public void ClosePort()
        {
            _Serial.Close();
        }


        public void InitiateCallbacks()
        {
            _Serial.DataReceived += _Serial_DatatReceived;
        }

        private void _Serial_DatatReceived(object sender, SerialDataReceivedEventArgs e)
        {
            readSerial();
        }


        static void Main(string[] args)
        {
            Uri baseAddress = new Uri("http://localhost:8080/SerialSwitch");
            SerialSwitchServiceHost prog = new SerialSwitchServiceHost();
            SerialPort port = new SerialPort();
            port.PortName = "COM3";
            port.PortName = "/dev/ttyACM0";
            port.BaudRate = 9600;
            port.Parity = Parity.None;
            port.DataBits = 8;
            port.StopBits = StopBits.One;
            port.ReadTimeout = SerialPort.InfiniteTimeout;
            port.WriteTimeout = 500;

            prog.setPort(port);
            prog.ClosePort();
            prog.OpenPort();

            //prog.InitiateCallbacks();

            Thread threadRec = new Thread(new ThreadStart(prog.readSerial));
            threadRec.Start();


            // Create the ServiceHost.
            // attempt 1
            //using (ServiceHost host = new ServiceHost(typeof(SerialSwitchServiceHost), baseAddress))
            //{
            var binding = new BasicHttpBinding();
            using (ServiceHost host = new ServiceHost(typeof(SerialSwitchServiceHost)))
            {

                //attempt #2
                host.AddServiceEndpoint(typeof(IArduinoSelfHost), binding, baseAddress);

                // Enable metadata publishing.
                //attempt 1
                //ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
                //smb.HttpGetEnabled = true;
                ////smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
                //host.Description.Behaviors.Add(smb);


                // Open the ServiceHost to start listening for messages. Since
                // no endpoints are explicitly configured, the runtime will create
                // one endpoint per base address for each service contract implemented
                // by the service.
                host.Open();


                Console.WriteLine("The service is ready at {0}", baseAddress);
                Console.WriteLine("Press <Enter> to stop the service.");
                Console.ReadLine();


                // Close the ServiceHost.
                host.Close();
                prog.ClosePort();
            }
        }





    }
    //}
}
