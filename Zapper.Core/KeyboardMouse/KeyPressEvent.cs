namespace Zapper.Core.KeyboardMouse
{
    public readonly struct KeyPressEvent
    {
        public KeyPressEvent(EventCode code, KeyState state)
        {
            Code = code;
            State = state;
        }

        public EventCode Code { get; }
        
        public KeyState State { get; }
    }
}