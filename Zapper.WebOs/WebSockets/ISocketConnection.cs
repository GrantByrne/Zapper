using System;

namespace Zapper.WebOs.WebSockets
{
    internal interface ISocketConnection
    {
        string Url { get; }
        void Connect(string url);
        bool IsAlive { get; }
        event EventHandler<SocketMessageEventArgs> OnMessage;
        void Send(string content);
        void Close();
    }
}