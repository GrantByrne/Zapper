namespace Zapper.Services;

public class BluetoothRemoteEventArgs : EventArgs
{
    public string ButtonCode { get; }
    public DateTime Timestamp { get; }

    public BluetoothRemoteEventArgs(string buttonCode)
    {
        ButtonCode = buttonCode;
        Timestamp = DateTime.UtcNow;
    }
}