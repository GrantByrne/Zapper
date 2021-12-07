using System;

namespace Zapper.WebOs.WebSockets
{
    public class SocketMessageEventArgs : EventArgs
    {
        internal SocketMessageEventArgs(string data)
        {
            Data = data;
        }

        public string Data { get; }
    }
}
