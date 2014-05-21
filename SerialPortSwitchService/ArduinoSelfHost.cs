using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrewduinoCatalogLib;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.IO.Ports;

namespace SerialPortSwitchService
{
    [ServiceContract]
    interface ArduinoSelfHost : IArduinoSerial
    {
        [OperationContract]
        string GetRawStatus();
        [OperationContract]
        Dictionary<string, decimal> GetStatus();
        [OperationContract]
        void SendCommand(ArduinoCommands.CommandTypes cmd, string text);
        [OperationContract]
        Dictionary<string, decimal> SendCommandWithResponse(ArduinoCommands.CommandTypes cmd, string text);
        [OperationContract]
        void UpdateStatus();
    }
}
