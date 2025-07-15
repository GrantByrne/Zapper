namespace Zapper.Device.Bluetooth;

public interface IBluetoothHidController
{
    Task<bool> SendKey(string deviceAddress, HidKeyCode keyCode, CancellationToken cancellationToken = default);
    Task<bool> SendKeySequence(string deviceAddress, HidKeyCode[] keyCodes, int delayMs = 50, CancellationToken cancellationToken = default);
    Task<bool> SendText(string deviceAddress, string text, CancellationToken cancellationToken = default);
    Task<bool> Connect(string deviceAddress, CancellationToken cancellationToken = default);
    Task<bool> Disconnect(string deviceAddress, CancellationToken cancellationToken = default);
    Task<bool> IsConnected(string deviceAddress, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetConnectedDevices(CancellationToken cancellationToken = default);
}