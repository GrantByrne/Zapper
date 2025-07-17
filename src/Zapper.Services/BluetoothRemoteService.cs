using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Zapper.Core.Models;
using Zapper.Device.Bluetooth;

namespace Zapper.Services;

public class BluetoothRemoteService : IBluetoothRemoteService
{
    private readonly IBluetoothHidServer _hidServer;
    private readonly ILogger<BluetoothRemoteService> _logger;
    private readonly IHubContext<ZapperSignalR>? _hubContext;
    private readonly Dictionary<string, BluetoothHost> _pairedHosts = new();
    private string? _currentRemoteName;
    private RemoteDeviceType _currentDeviceType;

    public BluetoothRemoteService(
        IBluetoothHidServer hidServer,
        ILogger<BluetoothRemoteService> logger,
        IHubContext<ZapperSignalR>? hubContext = null)
    {
        _hidServer = hidServer ?? throw new ArgumentNullException(nameof(hidServer));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _hubContext = hubContext;

        _hidServer.ClientConnected += OnClientConnected;
        _hidServer.ClientDisconnected += OnClientDisconnected;
    }

    public event EventHandler<BluetoothRemoteEventArgs>? RemoteButtonPressed;
    public event EventHandler<BluetoothRemoteConnectionEventArgs>? RemoteConnected;
    public event EventHandler<BluetoothRemoteConnectionEventArgs>? RemoteDisconnected;

    public bool IsAdvertising => _hidServer.IsAdvertising;
    public string? ConnectedHostAddress => _hidServer.ConnectedClientAddress;
    public string? ConnectedHostName => GetConnectedHostName();

    public async Task<bool> StartAdvertisingAsync(string remoteName, RemoteDeviceType deviceType, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting Bluetooth remote advertising as '{Name}' ({Type})", remoteName, deviceType);

            _currentRemoteName = remoteName;
            _currentDeviceType = deviceType;

            var hidDeviceType = deviceType switch
            {
                RemoteDeviceType.GameController => HidDeviceType.Gamepad,
                RemoteDeviceType.MediaRemote => HidDeviceType.Remote,
                _ => HidDeviceType.Remote
            };

            return await _hidServer.StartAdvertising(remoteName, hidDeviceType, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start Bluetooth remote advertising");
            return false;
        }
    }

    public async Task<bool> StopAdvertisingAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Stopping Bluetooth remote advertising");
            return await _hidServer.StopAdvertising(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop Bluetooth remote advertising");
            return false;
        }
    }

    public Task<RemoteStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        var connectedHost = ConnectedHostAddress != null ? _pairedHosts.GetValueOrDefault(ConnectedHostAddress) : null;

        return Task.FromResult(new RemoteStatus
        {
            IsAdvertising = IsAdvertising,
            RemoteName = _currentRemoteName,
            DeviceType = _currentDeviceType,
            ConnectedHostAddress = ConnectedHostAddress,
            ConnectedHostName = connectedHost?.Name ?? ConnectedHostName,
            ConnectedAt = connectedHost?.LastConnected
        });
    }

    public Task<IEnumerable<BluetoothHost>> GetPairedHostsAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_pairedHosts.Values.AsEnumerable());
    }

    public Task<bool> RemovePairedHostAsync(string hostAddress, CancellationToken cancellationToken = default)
    {
        if (_pairedHosts.Remove(hostAddress))
        {
            _logger.LogInformation("Removed paired host: {Address}", hostAddress);
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

    public async Task<bool> SendButtonPress(string buttonCode, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!Enum.TryParse<HidKeyCode>(buttonCode, out var keyCode))
            {
                _logger.LogWarning("Unknown button code: {Code}", buttonCode);
                return false;
            }

            // Send key press
            if (!await _hidServer.SendKeyPress(keyCode, cancellationToken))
            {
                return false;
            }

            // Small delay between press and release
            await Task.Delay(50, cancellationToken);

            // Send key release
            if (!await _hidServer.SendKeyRelease(keyCode, cancellationToken))
            {
                return false;
            }

            // Raise event
            RemoteButtonPressed?.Invoke(this, new BluetoothRemoteEventArgs(buttonCode));

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send button press: {Code}", buttonCode);
            return false;
        }
    }

    private async void OnClientConnected(object? sender, BluetoothHidConnectionEventArgs e)
    {
        // Update paired hosts
        if (!_pairedHosts.ContainsKey(e.ClientAddress))
        {
            _pairedHosts[e.ClientAddress] = new BluetoothHost
            {
                Address = e.ClientAddress,
                Name = e.ClientName,
                IsPaired = true
            };
        }

        _pairedHosts[e.ClientAddress].LastConnected = e.ConnectedAt;

        _logger.LogInformation("Bluetooth remote connected to host: {Name} ({Address})",
            e.ClientName ?? "Unknown", e.ClientAddress);

        RemoteConnected?.Invoke(this, new BluetoothRemoteConnectionEventArgs(e.ClientAddress, e.ClientName));

        // Send SignalR notification
        if (_hubContext != null)
        {
            await _hubContext.Clients.All.SendAsync("BluetoothRemoteConnected", new
            {
                HostAddress = e.ClientAddress,
                HostName = e.ClientName,
                ConnectedAt = e.ConnectedAt
            });
        }
    }

    private async void OnClientDisconnected(object? sender, BluetoothHidConnectionEventArgs e)
    {
        _logger.LogInformation("Bluetooth remote disconnected from host: {Address}", e.ClientAddress);
        RemoteDisconnected?.Invoke(this, new BluetoothRemoteConnectionEventArgs(e.ClientAddress));

        // Send SignalR notification
        if (_hubContext != null)
        {
            await _hubContext.Clients.All.SendAsync("BluetoothRemoteDisconnected", new
            {
                HostAddress = e.ClientAddress
            });
        }
    }

    private string? GetConnectedHostName()
    {
        if (ConnectedHostAddress == null)
            return null;

        return _pairedHosts.TryGetValue(ConnectedHostAddress, out var host) ? host.Name : null;
    }
}