namespace Zapper.Device.Bluetooth;

public interface IBluetoothHidServer
{
    event EventHandler<BluetoothHidConnectionEventArgs>? ClientConnected;
    event EventHandler<BluetoothHidConnectionEventArgs>? ClientDisconnected;

    bool IsAdvertising { get; }
    string? ConnectedClientAddress { get; }

    Task<bool> StartAdvertising(string deviceName, HidDeviceType deviceType, CancellationToken cancellationToken = default);
    Task<bool> StopAdvertising(CancellationToken cancellationToken = default);
    Task<bool> SendHidReport(byte[] report, CancellationToken cancellationToken = default);
    Task<bool> SendKeyPress(HidKeyCode keyCode, CancellationToken cancellationToken = default);
    Task<bool> SendKeyRelease(HidKeyCode keyCode, CancellationToken cancellationToken = default);
    Task<bool> IsClientConnected(CancellationToken cancellationToken = default);
}

public enum HidDeviceType
{
    Remote,
    Gamepad,
    Keyboard
}

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