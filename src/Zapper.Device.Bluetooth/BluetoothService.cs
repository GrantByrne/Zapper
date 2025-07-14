using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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

public class BluetoothService : BackgroundService, IBluetoothService
{
    private readonly ILogger<BluetoothService> _logger;
    private readonly BlueZAdapter _adapter;
    private readonly object _lock = new();

    public BluetoothService(ILogger<BluetoothService> logger, ILoggerFactory loggerFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        var loggerFactoryNotNull = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        _adapter = new BlueZAdapter(loggerFactoryNotNull.CreateLogger<BlueZAdapter>());
        
        // Forward events from adapter
        _adapter.DeviceFound += (s, e) => DeviceFound?.Invoke(this, e);
        _adapter.DeviceConnected += (s, e) => DeviceConnected?.Invoke(this, e);
        _adapter.DeviceDisconnected += (s, e) => DeviceDisconnected?.Invoke(this, e);
    }

    public event EventHandler<BluetoothDeviceEventArgs>? DeviceFound;
    public event EventHandler<BluetoothDeviceEventArgs>? DeviceConnected;
    public event EventHandler<BluetoothDeviceEventArgs>? DeviceDisconnected;

    public bool IsInitialized => _adapter.IsInitialized;
    public bool IsDiscovering => _adapter.IsDiscovering;
    public bool IsPowered => _adapter.IsPowered;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (IsInitialized)
            {
                _logger.LogInformation("Bluetooth service is already initialized");
                return;
            }
        }

        try
        {
            _logger.LogInformation("Initializing Bluetooth service...");
            await _adapter.InitializeAsync(cancellationToken);
            _logger.LogInformation("Bluetooth service initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Bluetooth service");
            throw;
        }
    }

    public Task<bool> SetPoweredAsync(bool powered, CancellationToken cancellationToken = default)
    {
        return _adapter.SetPoweredAsync(powered, cancellationToken);
    }

    public Task<bool> StartDiscoveryAsync(CancellationToken cancellationToken = default)
    {
        return _adapter.StartDiscoveryAsync(cancellationToken);
    }

    public Task<bool> StopDiscoveryAsync(CancellationToken cancellationToken = default)
    {
        return _adapter.StopDiscoveryAsync(cancellationToken);
    }

    public Task<IEnumerable<BluetoothDeviceInfo>> GetDevicesAsync(CancellationToken cancellationToken = default)
    {
        return _adapter.GetDevicesAsync(cancellationToken);
    }

    public Task<BluetoothDeviceInfo?> GetDeviceAsync(string address, CancellationToken cancellationToken = default)
    {
        return _adapter.GetDeviceAsync(address, cancellationToken);
    }

    public Task<bool> PairDeviceAsync(string address, CancellationToken cancellationToken = default)
    {
        return _adapter.PairDeviceAsync(address, cancellationToken);
    }

    public Task<bool> ConnectDeviceAsync(string address, CancellationToken cancellationToken = default)
    {
        return _adapter.ConnectDeviceAsync(address, cancellationToken);
    }

    public Task<bool> DisconnectDeviceAsync(string address, CancellationToken cancellationToken = default)
    {
        return _adapter.DisconnectDeviceAsync(address, cancellationToken);
    }

    public async Task<bool> RemoveDeviceAsync(string address, CancellationToken cancellationToken = default)
    {
        try
        {
            // First disconnect if connected
            await DisconnectDeviceAsync(address, cancellationToken);
            
            // Note: Linux.Bluetooth doesn't expose a direct remove method in the current version
            // This would typically require calling the RemoveDevice method on the adapter
            // For now, we'll log this limitation
            _logger.LogWarning("Device removal not implemented in current Linux.Bluetooth version. Device {Address} disconnected but not removed.", address);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove device {Address}", address);
            return false;
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await InitializeAsync(stoppingToken);
            
            // Keep the service running
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Bluetooth service stopped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bluetooth service encountered an error");
        }
        finally
        {
            if (IsDiscovering)
            {
                await StopDiscoveryAsync();
            }
        }
    }

    public override void Dispose()
    {
        _adapter?.Dispose();
        base.Dispose();
    }
}