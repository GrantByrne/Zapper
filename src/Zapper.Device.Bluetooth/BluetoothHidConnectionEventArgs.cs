namespace Zapper.Device.Bluetooth;

public class BluetoothHidConnectionEventArgs : EventArgs
{
    public string ClientAddress { get; }
    public string? ClientName { get; }
    public DateTime ConnectedAt { get; }

    public BluetoothHidConnectionEventArgs(string clientAddress, string? clientName = null)
    {
        ClientAddress = clientAddress;
        ClientName = clientName;
        ConnectedAt = DateTime.UtcNow;
    }
}