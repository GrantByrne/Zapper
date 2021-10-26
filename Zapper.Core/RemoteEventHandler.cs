using System;
using System.Collections.Generic;
using Zapper.Core.KeyboardMouse;

namespace Zapper.Core
{
    public class RemoteEventHandler : IRemoteEventHandler
    {
        private readonly Dictionary<(EventCode, KeyState), Action> _actions;

        public RemoteEventHandler(IAggregateInputReader aggregateInputReader)
        {
            _actions = new Dictionary<(EventCode, KeyState), Action>();
            
            aggregateInputReader.OnKeyPress += HandleInput;
        }

        private void HandleInput(KeyPressEvent e)
        {
            var key = (e.Code, e.State);

            if(_actions.TryGetValue(key, out var action))
            {
                action();
            }
        }

        public void RegisterAction(EventCode code, Action action)
        {
            _actions[(code, KeyState.KeyDown)] = action;
        }

        public void RemoveAction(EventCode code)
        {
            _actions.Remove((code, KeyState.KeyDown));
        }

        public void ClearActions()
        {
            _actions.Clear();
        }
    }
}