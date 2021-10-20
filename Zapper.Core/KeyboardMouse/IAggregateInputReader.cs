namespace Zapper.Core.KeyboardMouse
{
    public interface IAggregateInputReader
    {
        event InputReader.RaiseKeyPress OnKeyPress;
    }
}