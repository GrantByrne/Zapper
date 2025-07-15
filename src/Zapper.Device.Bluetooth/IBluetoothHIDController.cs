namespace Zapper.Device.Bluetooth;

public interface IBluetoothHidController
{
    Task<bool> SendKeyAsync(string deviceAddress, HidKeyCode keyCode, CancellationToken cancellationToken = default);
    Task<bool> SendKeySequenceAsync(string deviceAddress, HidKeyCode[] keyCodes, int delayMs = 50, CancellationToken cancellationToken = default);
    Task<bool> SendTextAsync(string deviceAddress, string text, CancellationToken cancellationToken = default);
    Task<bool> ConnectAsync(string deviceAddress, CancellationToken cancellationToken = default);
    Task<bool> DisconnectAsync(string deviceAddress, CancellationToken cancellationToken = default);
    Task<bool> IsConnectedAsync(string deviceAddress, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetConnectedDevicesAsync(CancellationToken cancellationToken = default);
}