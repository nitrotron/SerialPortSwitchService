using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SerialPortSwitchService
{
    class BadOneWireException : Exception
    {
        public BadOneWireException()
        {
        }
        public BadOneWireException(string message)
            : base(message)
        {
        }
        public BadOneWireException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
    
}
