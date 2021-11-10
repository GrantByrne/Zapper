namespace Zapper.Core.KeyboardMouse.Abstract
{
    public interface IAggregateInputReader
    {
        event InputReader.RaiseKeyPress OnKeyPress;
    }
}