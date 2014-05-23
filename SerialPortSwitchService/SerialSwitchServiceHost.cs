using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrewduinoCatalogLib;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.IO.Ports;
using System.Threading;

namespace SerialPortSwitchService
{


    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    class SerialSwitchServiceHost : IArduinoSelfHost
    {
        private static Dictionary<string, decimal> _Status;
        Dictionary<string, decimal> Status
        {
            get { return _Status; }
            set { _Status = value; }
        }
        private static SerialPort _Serial;
        public SerialSwitchServiceHost()
        {
            _Status = new Dictionary<string, decimal>();
        }

        private void setPort(SerialPort port)
        {
            _Serial = port;
        }

        public string GetRawStatus()
        {
            return string.Empty;
        }

        public Dictionary<string, decimal> GetStatus()
        {
            return Status;
        }

        //public void SendCommand(ArduinoCommands.CommandTypes cmd, string text)
        //{
        //    StringBuilder sendCmd = new StringBuilder();

        //    sendCmd.Append((int)cmd);
        //    if (!string.IsNullOrEmpty(text))
        //        sendCmd.Append("," + text + ";");
        //    else
        //        sendCmd.Append(";");


        //    if (sendCmd != null && !String.IsNullOrEmpty(sendCmd.ToString()))
        //    {
        //        //if (!IsOpen) Open();
        //        // FIXTHIS there was a problem with the serial not being open
        //        _Serial.WriteLine(sendCmd.ToString());
        //    }
        //}


        //public Dictionary<string, decimal> SendCommandWithResponse(ArduinoCommands.CommandTypes cmd, string text)
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

            response.Append(_Serial.ReadTo(";"));
            response = response.Replace("\r", "");
            response = response.Replace("\n", "");
            response = response.Replace(" ", "");



            Console.WriteLine(response.ToString());
            Dictionary<string, decimal> responseDictionary = parseVaribles(response.ToString());

            foreach (var item in responseDictionary)
            {
                _Status[item.Key] = item.Value;
            }

        }
        public Dictionary<string, decimal> parseVaribles(string response)
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

            Dictionary<string, decimal> dict = new Dictionary<string, decimal>();

            string[] pairs = message.Split(',');

            foreach (string textValue in pairs)
            {
                string[] pair = textValue.Split('|');

                if (pair.Length != 2)
                    break;

                decimal temp;
                decimal.TryParse(pair[1], out temp);

                dict.Add(pair[0], temp);
            }


            return dict;
        }

        public void OpenPort()
        {
            if (!_Serial.IsOpen)
                _Serial.Open();

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
            prog.OpenPort();
            prog.InitiateCallbacks();

            // Create the ServiceHost.
            // attempt 1
            using (ServiceHost host = new ServiceHost(typeof(SerialSwitchServiceHost), baseAddress))
            {
                //var binding = new BasicHttpBinding();
                //using (ServiceHost host = new ServiceHost(typeof(SerialSwitchServiceHost)))
                //{

                ////attempt #2
                // host.AddServiceEndpoint(typeof(IArduinoSelfHost), binding, baseAddress);

                // Enable metadata publishing.
                ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
                smb.HttpGetEnabled = true;
                //smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
                host.Description.Behaviors.Add(smb);


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
