using System;

namespace Zapper.Core.Keyboard
{
    public class KeyPressEvent : EventArgs
    {
        public KeyPressEvent(Keycode code, KeyState state)
        {
            Code = code;
            State = state;
        }

        public Keycode Code { get; }
        
        public KeyState State { get; }
    }
}