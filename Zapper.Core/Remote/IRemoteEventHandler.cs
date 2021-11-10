using System;
using Zapper.Core.KeyboardMouse;

namespace Zapper.Core.Remote
{
    public interface IRemoteEventHandler
    {
        void RegisterAction(EventCode code, Action action);
        
        void RemoveAction(EventCode code);

        void ClearActions();
    }
}