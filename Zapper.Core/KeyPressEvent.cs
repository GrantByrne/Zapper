using System;

namespace Zapper.Core
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