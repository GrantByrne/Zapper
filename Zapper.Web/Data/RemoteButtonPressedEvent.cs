using System;

namespace Zapper.Web.Data
{
    public class RemoteButtonPressedEvent : EventArgs
    {
        public RemoteButtonPressedEvent(RemoteButton remoteButton)
        {
            RemoteButton = remoteButton;
        }
        
        public RemoteButton RemoteButton { get; set; }
    }
}