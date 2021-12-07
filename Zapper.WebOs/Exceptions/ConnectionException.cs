using System;

namespace Zapper.WebOs.Exceptions
{
    public class ConnectionException : Exception
    {
        public ConnectionException(string message) : base(message){}
    }
}
