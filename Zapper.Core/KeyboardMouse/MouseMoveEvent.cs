namespace Zapper.Core.KeyboardMouse
{
    public readonly struct MouseMoveEvent
    {
        public MouseMoveEvent(MouseAxis axis, int amount)
        {
            Axis = axis;
            Amount = amount;
        }
        
        public MouseAxis Axis { get; }
        
        public int Amount { get; }
    }
}