using System;
using System.Collections.Generic;
using Zapper.Core.KeyboardMouse;

namespace Zapper.Core
{
    public class RemoteEventHandler
    {
        private readonly IAggregateInputReader _aggregateInputReader;
        private readonly Dictionary<(EventCode, KeyState), Action> _actions;

        public RemoteEventHandler(IAggregateInputReader aggregateInputReader)
        {
            _aggregateInputReader = aggregateInputReader;
            
            _actions = new Dictionary<(EventCode, KeyState), Action>();
            
            _aggregateInputReader.OnKeyPress += HandleInput;
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
    }
}