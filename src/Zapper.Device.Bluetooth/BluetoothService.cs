using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Zapper.Device.Bluetooth;

public class BluetoothService : BackgroundService, IBluetoothService
{
    private readonly ILogger<BluetoothService> _logger;
    private readonly BlueZAdapter _adapter;
    private readonly Lock _lock = new();

    public BluetoothService(ILogger<BluetoothService> logger, ILoggerFactory loggerFactory)
    {
        _logger = logger;
        _adapter = new BlueZAdapter(loggerFactory.CreateLogger<BlueZAdapter>());

        _adapter.DeviceFound += (_, e) => DeviceFound?.Invoke(this, e);
        _adapter.DeviceConnected += (_, e) => DeviceConnected?.Invoke(this, e);
        _adapter.DeviceDisconnected += (_, e) => DeviceDisconnected?.Invoke(this, e);
    }

    public event EventHandler<BluetoothDeviceEventArgs>? DeviceFound;
    public event EventHandler<BluetoothDeviceEventArgs>? DeviceConnected;
    public event EventHandler<BluetoothDeviceEventArgs>? DeviceDisconnected;

    public bool IsInitialized => _adapter.IsInitialized;
    public bool IsDiscovering => _adapter.IsDiscovering;
    public bool IsPowered => _adapter.IsPowered;

    public async Task Initialize(CancellationToken cancellationToken = default)
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

    public async Task<bool> SetPowered(bool powered, CancellationToken cancellationToken = default)
    {
        if (!IsInitialized)
        {
            _logger.LogWarning("Cannot set power state: Bluetooth adapter not initialized");
            return false;
        }

        return await _adapter.SetPoweredAsync(powered, cancellationToken);
    }

    public async Task<bool> StartDiscovery(CancellationToken cancellationToken = default)
    {
        if (!IsInitialized)
        {
            _logger.LogWarning("Cannot start discovery: Bluetooth adapter not initialized");
            return false;
        }

        return await _adapter.StartDiscoveryAsync(cancellationToken);
    }

    public async Task<bool> StopDiscovery(CancellationToken cancellationToken = default)
    {
        if (!IsInitialized)
        {
            _logger.LogWarning("Cannot stop discovery: Bluetooth adapter not initialized");
            return false;
        }

        return await _adapter.StopDiscoveryAsync(cancellationToken);
    }

    public async Task<IEnumerable<BluetoothDeviceInfo>> GetDevices(CancellationToken cancellationToken = default)
    {
        if (!IsInitialized)
        {
            _logger.LogWarning("Cannot get devices: Bluetooth adapter not initialized");
            return [];
        }

        return await _adapter.GetDevicesAsync(cancellationToken);
    }

    public async Task<BluetoothDeviceInfo?> GetDevice(string address, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(address))
        {
            _logger.LogWarning("Cannot get device: address is null or empty");
            return null;
        }

        if (!IsInitialized)
        {
            _logger.LogWarning("Cannot get device: Bluetooth adapter not initialized");
            return null;
        }

        return await _adapter.GetDeviceAsync(address, cancellationToken);
    }

    public async Task<bool> PairDevice(string address, CancellationToken cancellationToken = default)
    {
        if (!IsInitialized)
        {
            _logger.LogWarning("Cannot pair device: Bluetooth adapter not initialized");
            return false;
        }

        return await _adapter.PairDeviceAsync(address, cancellationToken);
    }

    public async Task<bool> ConnectDevice(string address, CancellationToken cancellationToken = default)
    {
        if (!IsInitialized)
        {
            _logger.LogWarning("Cannot connect device: Bluetooth adapter not initialized");
            return false;
        }

        return await _adapter.ConnectDeviceAsync(address, cancellationToken);
    }

    public async Task<bool> DisconnectDevice(string address, CancellationToken cancellationToken = default)
    {
        if (!IsInitialized)
        {
            _logger.LogWarning("Cannot disconnect device: Bluetooth adapter not initialized");
            return false;
        }

        return await _adapter.DisconnectDeviceAsync(address, cancellationToken);
    }

    public async Task<bool> RemoveDevice(string address, CancellationToken cancellationToken = default)
    {
        try
        {
            await DisconnectDevice(address, cancellationToken);
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
            await Initialize(stoppingToken);

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
                await StopDiscovery(stoppingToken);
            }
        }
    }

    public override void Dispose()
    {
        _adapter?.Dispose();
        base.Dispose();
    }
}