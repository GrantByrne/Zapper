namespace Zapper.Device.Bluetooth;

public interface IBluetoothService
{
    event EventHandler<BluetoothDeviceEventArgs>? DeviceFound;
    event EventHandler<BluetoothDeviceEventArgs>? DeviceConnected;
    event EventHandler<BluetoothDeviceEventArgs>? DeviceDisconnected;

    bool IsInitialized { get; }
    bool IsDiscovering { get; }
    bool IsPowered { get; }

    Task Initialize(CancellationToken cancellationToken = default);
    Task<bool> SetPowered(bool powered, CancellationToken cancellationToken = default);
    Task<bool> StartDiscovery(CancellationToken cancellationToken = default);
    Task<bool> StopDiscovery(CancellationToken cancellationToken = default);
    Task<IEnumerable<BluetoothDeviceInfo>> GetDevices(CancellationToken cancellationToken = default);
    Task<BluetoothDeviceInfo?> GetDevice(string address, CancellationToken cancellationToken = default);
    Task<bool> PairDevice(string address, CancellationToken cancellationToken = default);
    Task<bool> ConnectDevice(string address, CancellationToken cancellationToken = default);
    Task<bool> DisconnectDevice(string address, CancellationToken cancellationToken = default);
    Task<bool> RemoveDevice(string address, CancellationToken cancellationToken = default);
}