using System;
using System.Collections.Generic;
using Zapper.Core.KeyboardMouse;

namespace Zapper.Core
{
    public class RemoteEventHandler : IDisposable
    {
        private readonly AggregateInputReader _aggregateInputReader;
        private readonly Dictionary<(EventCode, KeyState), Action> _actions;

        public RemoteEventHandler()
        {
            _aggregateInputReader = new AggregateInputReader();
            _actions = new Dictionary<(EventCode, KeyState), Action>();
            
            _aggregateInputReader.OnKeyPress += HandleInput;
            
            RegisterActions();
        }

        private void HandleInput(KeyPressEvent e)
        {
            var key = (e.Code, e.State);

            if(_actions.TryGetValue(key, out var action))
            {
                action();
            }
        }

        private void RegisterActions()
        {
            _actions.Add((EventCode.Up, KeyState.KeyDown), () => Console.WriteLine("Up"));
            _actions.Add((EventCode.Left, KeyState.KeyDown), () => Console.WriteLine("Left"));
            _actions.Add((EventCode.Right, KeyState.KeyDown), () => Console.WriteLine("Right"));
            _actions.Add((EventCode.Down, KeyState.KeyDown), () => Console.WriteLine("Down"));
            _actions.Add((EventCode.Enter, KeyState.KeyDown), () => Console.WriteLine("Enter"));
        }

        public void Dispose()
        {
            _aggregateInputReader.Dispose();
        }
    }
}