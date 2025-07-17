using Linux.Bluetooth;
using Linux.Bluetooth.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Zapper.Device.Bluetooth;

public class BluetoothHidServer : BackgroundService, IBluetoothHidServer
{
    private readonly ILogger<BluetoothHidServer> _logger;
    private readonly object _lock = new();
    private IAdapter1? _adapter;
#pragma warning disable CS0414
    private IGattService1? _hidService;
    private IGattCharacteristic1? _reportCharacteristic;
    private IGattCharacteristic1? _reportMapCharacteristic;
    private IGattCharacteristic1? _hidInfoCharacteristic;
    private IGattCharacteristic1? _controlPointCharacteristic;
#pragma warning restore CS0414
    private string? _connectedClientAddress;
    private HidDeviceType _currentDeviceType;
    private bool _isAdvertising;

    private const string HID_SERVICE_UUID = "00001812-0000-1000-8000-00805f9b34fb";
    private const string HID_INFORMATION_UUID = "00002a4a-0000-1000-8000-00805f9b34fb";
    private const string REPORT_MAP_UUID = "00002a4b-0000-1000-8000-00805f9b34fb";
    private const string HID_CONTROL_POINT_UUID = "00002a4c-0000-1000-8000-00805f9b34fb";
    private const string REPORT_UUID = "00002a4d-0000-1000-8000-00805f9b34fb";

    public BluetoothHidServer(ILogger<BluetoothHidServer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public event EventHandler<BluetoothHidConnectionEventArgs>? ClientConnected;
    public event EventHandler<BluetoothHidConnectionEventArgs>? ClientDisconnected;

    public bool IsAdvertising => _isAdvertising;
    public string? ConnectedClientAddress => _connectedClientAddress;

    public async Task<bool> StartAdvertising(string deviceName, HidDeviceType deviceType, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (_isAdvertising)
            {
                _logger.LogWarning("Already advertising as HID device");
                return false;
            }
        }

        try
        {
            _logger.LogInformation("Starting HID server advertising as '{Name}' ({Type})", deviceName, deviceType);

            if (_adapter == null)
            {
                await InitializeAdapter(cancellationToken);
            }

            _currentDeviceType = deviceType;

            // Set adapter properties
            await _adapter!.SetAsync("Discoverable", true);
            await _adapter.SetAsync("DiscoverableTimeout", 0u); // Always discoverable
            await _adapter.SetAsync("Pairable", true);
            await _adapter.SetAsync("PairableTimeout", 0u); // Always pairable

            // Set device name
            await _adapter.SetAsync("Alias", deviceName);

            // Create GATT services
            await CreateGattServices(deviceType, cancellationToken);

            // Start advertising
            await StartGattAdvertising(deviceName, cancellationToken);

            lock (_lock)
            {
                _isAdvertising = true;
            }

            _logger.LogInformation("HID server advertising started successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start HID server advertising");
            return false;
        }
    }

    public async Task<bool> StopAdvertising(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (!_isAdvertising)
            {
                return true;
            }
        }

        try
        {
            _logger.LogInformation("Stopping HID server advertising");

            // Stop advertising
            if (_adapter != null)
            {
                await _adapter.SetAsync("Discoverable", false);
                await _adapter.SetAsync("Pairable", false);
            }

            // Remove GATT services
            await RemoveGattServices(cancellationToken);

            lock (_lock)
            {
                _isAdvertising = false;
                _connectedClientAddress = null;
            }

            _logger.LogInformation("HID server advertising stopped");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop HID server advertising");
            return false;
        }
    }

    public async Task<bool> SendHidReport(byte[] report, CancellationToken cancellationToken = default)
    {
        if (_reportCharacteristic == null || string.IsNullOrEmpty(_connectedClientAddress))
        {
            _logger.LogWarning("Cannot send HID report: no client connected");
            return false;
        }

        try
        {
            await _reportCharacteristic.SetAsync("Value", report);

            // Notify the connected client
            var notifying = await _reportCharacteristic.GetAsync<bool>("Notifying");
            if (notifying)
            {
                _logger.LogDebug("Sent HID report: {Report}", BitConverter.ToString(report));
                return true;
            }
            else
            {
                _logger.LogWarning("HID report characteristic is not notifying");
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send HID report");
            return false;
        }
    }

    public async Task<bool> SendKeyPress(HidKeyCode keyCode, CancellationToken cancellationToken = default)
    {
        var report = _currentDeviceType switch
        {
            HidDeviceType.Remote => HidReportDescriptors.CreateRemoteKeyReport(keyCode, true),
            HidDeviceType.Keyboard => HidReportDescriptors.CreateKeyboardReport(keyCode, true),
            _ => null
        };

        if (report == null)
        {
            _logger.LogWarning("Cannot create key press report for device type {Type}", _currentDeviceType);
            return false;
        }

        return await SendHidReport(report, cancellationToken);
    }

    public async Task<bool> SendKeyRelease(HidKeyCode keyCode, CancellationToken cancellationToken = default)
    {
        var report = _currentDeviceType switch
        {
            HidDeviceType.Remote => HidReportDescriptors.CreateRemoteKeyReport(keyCode, false),
            HidDeviceType.Keyboard => HidReportDescriptors.CreateKeyboardReport(keyCode, false),
            _ => null
        };

        if (report == null)
        {
            _logger.LogWarning("Cannot create key release report for device type {Type}", _currentDeviceType);
            return false;
        }

        return await SendHidReport(report, cancellationToken);
    }

    public Task<bool> IsClientConnected(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(!string.IsNullOrEmpty(_connectedClientAddress));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await InitializeAdapter(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Bluetooth HID server stopped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bluetooth HID server encountered an error");
        }
        finally
        {
            if (_isAdvertising)
            {
                await StopAdvertising();
            }
        }
    }

    private async Task InitializeAdapter(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Initializing Bluetooth adapter for HID server");

        var adapters = await BlueZManager.GetAdaptersAsync();
        _adapter = adapters.FirstOrDefault();

        if (_adapter == null)
        {
            throw new InvalidOperationException("No Bluetooth adapters found");
        }

        var address = await _adapter.GetAddressAsync();
        _logger.LogInformation("Using Bluetooth adapter: {Address}", address);

        // Ensure adapter is powered
        if (!await _adapter.GetAsync<bool>("Powered"))
        {
            await _adapter.SetAsync("Powered", true);
        }
    }

    private Task CreateGattServices(HidDeviceType deviceType, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating GATT services for HID device type: {Type}", deviceType);

        // Note: The actual GATT service creation would require BlueZ GATT API
        // This is a placeholder for the actual implementation
        // In reality, you would need to use DBus to register GATT services

        // For now, we'll log what would be created
        _logger.LogWarning("GATT service creation not fully implemented. This requires BlueZ GATT Manager API");

        return Task.CompletedTask;
    }

    private Task RemoveGattServices(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Removing GATT services");

        // Placeholder for GATT service removal
        _hidService = null;
        _reportCharacteristic = null;
        _reportMapCharacteristic = null;
        _hidInfoCharacteristic = null;
        _controlPointCharacteristic = null;

        return Task.CompletedTask;
    }

    private Task StartGattAdvertising(string deviceName, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting GATT advertising for device: {Name}", deviceName);

        // Note: This would use BlueZ's LEAdvertisingManager1 interface
        // For now, basic discoverable mode is enabled above

        return Task.CompletedTask;
    }

    private void OnClientConnected(string clientAddress, string? clientName)
    {
        lock (_lock)
        {
            _connectedClientAddress = clientAddress;
        }

        _logger.LogInformation("HID client connected: {Name} ({Address})", clientName ?? "Unknown", clientAddress);
        ClientConnected?.Invoke(this, new BluetoothHidConnectionEventArgs(clientAddress, clientName));
    }

    private void OnClientDisconnected(string clientAddress)
    {
        lock (_lock)
        {
            _connectedClientAddress = null;
        }

        _logger.LogInformation("HID client disconnected: {Address}", clientAddress);
        ClientDisconnected?.Invoke(this, new BluetoothHidConnectionEventArgs(clientAddress));
    }

    public override void Dispose()
    {
        if (_isAdvertising)
        {
            _ = Task.Run(async () => await StopAdvertising());
        }

        base.Dispose();
    }
}