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