namespace Zapper.Services;

public class BluetoothRemoteConnectionEventArgs : EventArgs
{
    public string HostAddress { get; }
    public string? HostName { get; }
    public DateTime Timestamp { get; }

    public BluetoothRemoteConnectionEventArgs(string hostAddress, string? hostName = null)
    {
        HostAddress = hostAddress;
        HostName = hostName;
        Timestamp = DateTime.UtcNow;
    }
}