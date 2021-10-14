using System;
using System.Collections.Generic;

namespace Zapper.Core
{
    public class RemoteInputHandler : IDisposable
    {
        private readonly AllDeviceInputManager _allDeviceInputManager;
        private readonly Dictionary<(Keycode, KeyState), Action> _actions;

        public RemoteInputHandler()
        {
            _allDeviceInputManager = new AllDeviceInputManager();
            _actions = new Dictionary<(Keycode, KeyState), Action>();
            
            _allDeviceInputManager.OnKeyPress += HandleInput;
            
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
            _actions.Add((Keycode.Up, KeyState.KeyDown), () => Console.WriteLine("Up"));
            _actions.Add((Keycode.Left, KeyState.KeyDown), () => Console.WriteLine("Left"));
            _actions.Add((Keycode.Right, KeyState.KeyDown), () => Console.WriteLine("Right"));
            _actions.Add((Keycode.Down, KeyState.KeyDown), () => Console.WriteLine("Down"));
            _actions.Add((Keycode.Enter, KeyState.KeyDown), () => Console.WriteLine("Enter"));
        }

        public void Dispose()
        {
            _allDeviceInputManager.Dispose();
        }
    }
}