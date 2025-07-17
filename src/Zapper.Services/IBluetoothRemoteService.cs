using Zapper.Core.Models;

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

public class RemoteStatus
{
    public bool IsAdvertising { get; set; }
    public string? RemoteName { get; set; }
    public RemoteDeviceType DeviceType { get; set; }
    public string? ConnectedHostAddress { get; set; }
    public string? ConnectedHostName { get; set; }
    public DateTime? ConnectedAt { get; set; }
}

public class BluetoothHost
{
    public string Address { get; set; } = string.Empty;
    public string? Name { get; set; }
    public DateTime LastConnected { get; set; }
    public bool IsPaired { get; set; }
}

public enum RemoteDeviceType
{
    TVRemote,
    GameController,
    MediaRemote
}