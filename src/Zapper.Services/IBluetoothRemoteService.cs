namespace Zapper.Services;

public interface IBluetoothRemoteService
{
    event EventHandler<BluetoothRemoteEventArgs>? RemoteButtonPressed;
    event EventHandler<BluetoothRemoteConnectionEventArgs>? RemoteConnected;
    event EventHandler<BluetoothRemoteConnectionEventArgs>? RemoteDisconnected;

    bool IsAdvertising { get; }
    string? ConnectedHostAddress { get; }
    string? ConnectedHostName { get; }

    Task<bool> StartAdvertisingAsync(string remoteName, RemoteDeviceType deviceType, CancellationToken cancellationToken = default);
    Task<bool> StopAdvertisingAsync(CancellationToken cancellationToken = default);
    Task<RemoteStatus> GetStatusAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<BluetoothHost>> GetPairedHostsAsync(CancellationToken cancellationToken = default);
    Task<bool> RemovePairedHostAsync(string hostAddress, CancellationToken cancellationToken = default);
}