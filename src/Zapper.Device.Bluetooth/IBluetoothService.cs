namespace Zapper.Device.Bluetooth;

public interface IBluetoothService
{
    event EventHandler<BluetoothDeviceEventArgs>? DeviceFound;
    event EventHandler<BluetoothDeviceEventArgs>? DeviceConnected;
    event EventHandler<BluetoothDeviceEventArgs>? DeviceDisconnected;

    bool IsInitialized { get; }
    bool IsDiscovering { get; }
    bool IsPowered { get; }

    Task InitializeAsync(CancellationToken cancellationToken = default);
    Task<bool> SetPoweredAsync(bool powered, CancellationToken cancellationToken = default);
    Task<bool> StartDiscoveryAsync(CancellationToken cancellationToken = default);
    Task<bool> StopDiscoveryAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<BluetoothDeviceInfo>> GetDevicesAsync(CancellationToken cancellationToken = default);
    Task<BluetoothDeviceInfo?> GetDeviceAsync(string address, CancellationToken cancellationToken = default);
    Task<bool> PairDeviceAsync(string address, CancellationToken cancellationToken = default);
    Task<bool> ConnectDeviceAsync(string address, CancellationToken cancellationToken = default);
    Task<bool> DisconnectDeviceAsync(string address, CancellationToken cancellationToken = default);
    Task<bool> RemoveDeviceAsync(string address, CancellationToken cancellationToken = default);
}