﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using BrewduinoCatalogLib;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.IO.Ports;
using System.Threading;
using System.Globalization;
using System.ServiceModel.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SerialPortSwitchService
{


    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    class SerialSwitchServiceHost : IArduinoSelfHost
    {
        private static Dictionary<string, string> _Status;
        Dictionary<string, string> Status
        {
            get
            {
                return _Status;
            }
            set { _Status = value; }
        }
        private static SerialPort _Serial;
        private static int count;
        public static bool IsLinux
        {
            get
            {
                int p = (int)Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }
        public SerialSwitchServiceHost()
        {
            _Status = new Dictionary<string, string>();
        }

        private void setPort(SerialPort port)
        {

            Console.WriteLine("Initializing Count");
            count = 0;

            _Serial = port;
        }

        public string GetRawStatus()
        {
            return string.Empty;
        }

        public string GetStatus()
        {
            Console.WriteLine("you have gotten a getStatus");
            count = count + 1;
            Console.Write("count is now:" + count);
            CultureInfo ci = new CultureInfo("en-US");
            Status["ServerTime"] = DateTime.Now.ToString(ci);
            Console.WriteLine("We're about to do a little Json");
            var jsonSerializerSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
            Console.WriteLine("Done Setting up");
            string rStr =  JsonConvert.SerializeObject(Status, Formatting.Indented, jsonSerializerSettings);
            Console.WriteLine("Done serializing. The string is:");

            Console.WriteLine(rStr);
            return rStr;


            //return Status;
        }

        public void SendCommand(string arduinoCommands, string text)
        {
            StringBuilder sendCmd = new StringBuilder();
            int aCmd;
            int.TryParse(arduinoCommands, out aCmd);


            sendCmd.Append(aCmd);
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
                    //response = response.Replace(" ", "");


                }
                catch
                {
                    //Close();
                    //return string.Empty;
                }

                //Console.WriteLine(response.ToString());
                Dictionary<string, string> responseDictionary = parseVaribles(response.ToString());


                foreach (var item in responseDictionary)
                {
                    _Status[item.Key] = item.Value;
                    if (item.Key == "NoAddress")
                        throw new BadOneWireException("Found no address for " + item.Value);
                }
            }

        }
        public Dictionary<string, string> parseVaribles(string response)
        {
            string[] pStrings;
            string message;
            if (response.Contains('@'))
            {
                pStrings = response.Split('@');
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

            Dictionary<string, string> dict = new Dictionary<string, string>();

            string[] pairs = message.Split(',');

            foreach (string textValue in pairs)
            {
                string[] pair = textValue.Split('|');

                if (pair.Length != 2)
                    break;

                //float temp;
                //float.TryParse(pair[1], out temp);

                if (pair[0] == "ClearTimers")
                    ClearTimers();
                else
                    dict.Add(pair[0], pair[1]);
            }


            return dict;
        }

        private void SetTestData()
        {
            _Status["Thermometer0"] = "99.5";
            _Status["ThermometerHighAlarm0"] = "212";
            _Status["ThermometerLowAlarm0"] = "14";
        }

        private void ClearTimers()
        {
            if (Status.ContainsKey("TotalTimers"))
            {
                int totalTimers;
                int.TryParse(Status["TotalTimers"], out totalTimers);
                for (int i = 0; i < totalTimers; i++)
                {
                    Status.Remove("Timer" + i);
                }
            }
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

        private void setInitialTime()
        {
            int setTimeCmdEnum = 24;
            StringBuilder cmd = new StringBuilder();
            CultureInfo ci = new CultureInfo("en-US");


            cmd.Append(DateTime.Now.Hour.ToString(ci) + ",");
            cmd.Append(DateTime.Now.Minute.ToString(ci) + ",");
            cmd.Append(DateTime.Now.Second.ToString(ci) + ",");
            cmd.Append(DateTime.Now.Month.ToString(ci) + ",");
            cmd.Append(DateTime.Now.Day.ToString(ci) + ",");
            cmd.Append(DateTime.Now.Year.ToString(ci));

            Console.WriteLine("Sending: " + setTimeCmdEnum + "," + cmd);
            System.Threading.Thread.Sleep(5000);
            SendCommand(setTimeCmdEnum.ToString(), cmd.ToString());
        }


        static void Main(string[] args)
        {
            Uri baseAddress = new Uri("http://localhost:8080/SerialSwitch");
            SerialSwitchServiceHost prog = new SerialSwitchServiceHost();
            SerialPort port = new SerialPort();
            port.PortName = "COM3";
            if (IsLinux)
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

            prog.setInitialTime();

            Thread threadRec = new Thread(new ThreadStart(prog.readSerial));
            threadRec.Start();


            // Create the ServiceHost.
            // attempt 1
            //using (ServiceHost host = new ServiceHost(typeof(SerialSwitchServiceHost), baseAddress))
            //{
            //            var binding = new BasicHttpBinding();
            //            using (ServiceHost host = new ServiceHost(typeof(SerialSwitchServiceHost)))
            //using (WebServiceHost host = new WebServiceHost(typeof(SerialSwitchServiceHost), new Uri("http://localhost:5000"))
            //            {
            WebServiceHost host = new WebServiceHost(typeof(SerialSwitchServiceHost), baseAddress);
            ServiceEndpoint ep = host.AddServiceEndpoint(typeof(IArduinoSelfHost), new WebHttpBinding(), "");
            ServiceDebugBehavior stp = host.Description.Behaviors.Find<ServiceDebugBehavior>();
            stp.HttpHelpPageEnabled = false;
            host.Open();

            prog.SetTestData();
            //attempt #2
            //      host.AddServiceEndpoint(typeof(IArduinoSelfHost), binding, baseAddress);


            // Open the ServiceHost to start listening for messages. Since
            // no endpoints are explicitly configured, the runtime will create
            // one endpoint per base address for each service contract implemented
            // by the service.
            //    host.Open();


            Console.WriteLine("The service is ready at {0}", baseAddress);
            Console.WriteLine("Press <Enter> to stop the service.");
            Console.ReadLine();


            // Close the ServiceHost.
            host.Close();
            //    prog.ClosePort();

        }

    }
    //}
}
