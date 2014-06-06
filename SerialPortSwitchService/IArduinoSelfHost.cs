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
    public interface IArduinoSelfHost //: IArduinoSerial
    {
        [OperationContract]
        string GetRawStatus();
        [OperationContract]
        Dictionary<string, float> GetStatus();
        [OperationContract]
        void SendCommand(int arduinoCommands, string text);
        //[OperationContract]
        //Dictionary<string, float> SendCommandWithResponse(int arduinoCommands, string text);
        [OperationContract]
        void UpdateStatus();
    }
}
