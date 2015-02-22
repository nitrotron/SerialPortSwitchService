using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrewduinoCatalogLib;
using System.ServiceModel;
//using System.ServiceModel.Description;
using System.ServiceModel.Web;
using System.IO.Ports;

namespace SerialPortSwitchService
{
    [ServiceContract]
    public interface IArduinoSelfHost //: IArduinoSerial
    {
        [OperationContract]
        string GetRawStatus();
        [OperationContract]
        [WebGet(UriTemplate = "GetStatus", ResponseFormat = WebMessageFormat.Json)]
        Dictionary<string, string> GetStatus();
        [OperationContract]
        [WebInvoke(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "SendCommand/{arduinoCommands}/{text}", Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped)]
        void SendCommand(string arduinoCommands, string text);
        //[OperationContract]
        //Dictionary<string, string> SendCommandWithResponse(int arduinoCommands, string text);
        [OperationContract]
        void UpdateStatus();
    }
}
