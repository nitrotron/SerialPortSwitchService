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
    class SerialSwitchServiceHost : SerialPort, ArduinoSelfHost
    {
        Dictionary<string, decimal> _Status;
        Dictionary<string, decimal> Status
        {
            get { return _Status; }
            set { _Status = value; }
        }
        public SerialSwitchServiceHost()
            : base()
        {

            _Status = new Dictionary<string, decimal>();

            this.PortName = "COM3";
            this.BaudRate = 9600;
            this.Parity = Parity.None;
            this.DataBits = 8;
            this.StopBits = StopBits.One;
            this.ReadTimeout = 5000;
            this.WriteTimeout = 500;


        }


        public string GetRawStatus()
        {
            return string.Empty;
        }

        public Dictionary<string, decimal> GetStatus()
        {
            return new Dictionary<string, decimal>();
        }

        public void SendCommand(ArduinoCommands.CommandTypes cmd, string text)
        {
            StringBuilder sendCmd = new StringBuilder();


            sendCmd.Append((int)cmd);
            if (!string.IsNullOrEmpty(text))
                sendCmd.Append("," + text + ";");
            else
                sendCmd.Append(";");


            if (sendCmd != null && !String.IsNullOrEmpty(sendCmd.ToString()))
            {
                //if (!IsOpen) Open();
                // FIXTHIS there was a problem with the serial not being open
                WriteLine(sendCmd.ToString());
            }
        }


        public Dictionary<string, decimal> SendCommandWithResponse(ArduinoCommands.CommandTypes cmd, string text)
        {
            //if (!IsOpen) Open();
            SendCommand(cmd, text);
            //StringBuilder response = new StringBuilder();

            //while (true)
            //{
            //    try
            //    {
            //        response.Append(ReadLine());
            //    }
            //    catch
            //    {
            //        //Close();
            //        return new Dictionary<string, decimal>();
            //    }
            //    if (response.ToString().Contains(";"))
            //        break;

            //}

            ////Close();

            //return parseVaribles(response.ToString());

            //todo, maybe wait for a status update.
            return Status;
        }

        public void UpdateStatus()
        {
            SendCommand(ArduinoCommands.CommandTypes.ReturnStatus, "");
        }

        public void readSerial()
        {
            StringBuilder response = new StringBuilder();
            while (true)
            {
                try
                {
                    response.Append(ReadTo(";"));
                }
                catch
                {
                    //Close();
                    //return string.Empty;
                }
                Console.WriteLine(response.ToString());
                Dictionary<string, decimal> responseDictionary = parseVaribles(response.ToString());
                //TODO need to merge this with the status
                foreach (var item in responseDictionary)
                {
                    _Status[item.Key] = item.Value;
                }
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
                //string value = pair[1];
                decimal temp;
                decimal.TryParse(pair[1], out temp);
                //int temp = (int)Convert.ToDecimal(value);
                //dict.Add(pair[0], (int)Convert.ToInt32(pair[1]));
                dict.Add(pair[0], temp);
            }


            return dict;
        }

        public void OpenPort()
        {
            if (!IsOpen) Open();
        }
        public void ClosePort()
        {
            Close();
        }

        //todo fix this
        class program
        {
            static void Main(string[] args)
            {
                Uri baseAddress = new Uri("http://localhost:8080/hello");
                SerialSwitchServiceHost prog = new SerialSwitchServiceHost();
                prog.OpenPort();

                //SerialPort port = new SerialPort("COM3", 9600, Parity.None, 8, StopBits.One);
                //HelloWorldService hws = new HelloWorldService();
                prog.hws.setPort(prog.port);

                Thread threadRec = new Thread(new ThreadStart(prog.readSerial));
                //threadRec.Start();

                // Create the ServiceHost.
                using (ServiceHost host = new ServiceHost(typeof(HelloWorldService), baseAddress))
                {
                    // Enable metadata publishing.
                    ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
                    smb.HttpGetEnabled = true;
                    smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
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
                }
            }
        }
    }
}
