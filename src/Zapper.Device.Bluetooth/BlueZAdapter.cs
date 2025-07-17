using Linux.Bluetooth;
using Linux.Bluetooth.Extensions;
using Microsoft.Extensions.Logging;

namespace Zapper.Device.Bluetooth;

public class BlueZAdapter(ILogger<BlueZAdapter> logger) : IDisposable
{
    private IAdapter1? _adapter;
    private readonly Dictionary<string, IDevice1> _devices = new();
    private readonly HashSet<string> _discoveredDevices = new();
    private CancellationTokenSource? _discoveryCts;
    private bool _disposed;

    public event EventHandler<BluetoothDeviceEventArgs>? DeviceFound;
    public event EventHandler<BluetoothDeviceEventArgs>? DeviceConnected;
    public event EventHandler<BluetoothDeviceEventArgs>? DeviceDisconnected;

    public bool IsInitialized => _adapter != null;
    public bool IsDiscovering { get; private set; }
    public bool IsPowered => _adapter?.GetAsync<bool>("Powered").GetAwaiter().GetResult() ?? false;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Initializing BlueZ adapter...");

            var adapters = await BlueZManager.GetAdaptersAsync();
            _adapter = adapters.FirstOrDefault();

            if (_adapter == null)
            {
                throw new InvalidOperationException("No Bluetooth adapters found on the system");
            }

            logger.LogInformation("Found Bluetooth adapter: {Address}", await _adapter.GetAddressAsync());

            if (!await _adapter.GetAsync<bool>("Powered"))
            {
                logger.LogInformation("Powering on Bluetooth adapter...");
                await _adapter.SetAsync("Powered", true);
            }

            logger.LogInformation("BlueZ adapter initialized successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to initialize BlueZ adapter");
            throw;
        }
    }

    public async Task<bool> SetPoweredAsync(bool powered, CancellationToken cancellationToken = default)
    {
        if (_adapter == null)
            throw new InvalidOperationException("Adapter not initialized");

        try
        {
            await _adapter.SetAsync("Powered", powered);
            logger.LogInformation("Bluetooth adapter power set to: {Powered}", powered);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to set adapter power state to {Powered}", powered);
            return false;
        }
    }

    public async Task<bool> StartDiscoveryAsync(CancellationToken cancellationToken = default)
    {
        if (_adapter == null)
            throw new InvalidOperationException("Adapter not initialized");

        try
        {
            if (IsDiscovering)
            {
                logger.LogWarning("Discovery is already running");
                return true;
            }

            await _adapter.StartDiscoveryAsync();
            IsDiscovering = true;
            logger.LogInformation("Started Bluetooth device discovery");

            // Log current discovery filters
            try
            {
                var discoveryFilter = await _adapter.GetDiscoveryFiltersAsync();
                logger.LogInformation("Discovery filters: {Filters}", string.Join(", ", discoveryFilter));
            }
            catch
            {
                logger.LogInformation("No discovery filters set");
            }

            // Start polling for new devices
            _discoveryCts = new CancellationTokenSource();
            _ = Task.Run(async () => await PollForDevicesAsync(_discoveryCts.Token));

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to start discovery");
            return false;
        }
    }

    public async Task<bool> StopDiscoveryAsync(CancellationToken cancellationToken = default)
    {
        if (_adapter == null)
            throw new InvalidOperationException("Adapter not initialized");

        try
        {
            if (!IsDiscovering)
            {
                logger.LogWarning("Discovery is not running");
                return true;
            }

            // Cancel polling
            _discoveryCts?.Cancel();
            _discoveryCts?.Dispose();
            _discoveryCts = null;

            await _adapter.StopDiscoveryAsync();
            IsDiscovering = false;
            _discoveredDevices.Clear();
            logger.LogInformation("Stopped Bluetooth device discovery");
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to stop discovery");
            return false;
        }
    }

    public async Task<IEnumerable<BluetoothDeviceInfo>> GetDevicesAsync(CancellationToken cancellationToken = default)
    {
        if (_adapter == null)
            throw new InvalidOperationException("Adapter not initialized");

        try
        {
            var devices = await _adapter.GetDevicesAsync();
            var deviceInfos = new List<BluetoothDeviceInfo>();

            foreach (var device in devices)
            {
                var deviceInfo = await CreateDeviceInfoAsync(device);
                deviceInfos.Add(deviceInfo);
            }

            return deviceInfos;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get devices");
            return [];
        }
    }

    public async Task<BluetoothDeviceInfo?> GetDeviceAsync(string address, CancellationToken cancellationToken = default)
    {
        if (_adapter == null)
            throw new InvalidOperationException("Adapter not initialized");

        try
        {
            var devices = await _adapter.GetDevicesAsync();
            var device = devices.FirstOrDefault(d => d.GetAsync<string>("Address").GetAwaiter().GetResult() == address);

            if (device == null)
                return null;

            return await CreateDeviceInfoAsync(device);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get device {Address}", address);
            return null;
        }
    }

    public async Task<bool> PairDeviceAsync(string address, CancellationToken cancellationToken = default)
    {
        if (_adapter == null)
            throw new InvalidOperationException("Adapter not initialized");

        try
        {
            var devices = await _adapter.GetDevicesAsync();
            var device = devices.FirstOrDefault(d => d.GetAsync<string>("Address").GetAwaiter().GetResult() == address);

            if (device == null)
            {
                logger.LogWarning("Device {Address} not found for pairing", address);
                return false;
            }

            if (await device.GetAsync<bool>("Paired"))
            {
                logger.LogInformation("Device {Address} is already paired", address);
                return true;
            }

            logger.LogInformation("Pairing with device {Address}...", address);
            await device.PairAsync();

            var isPaired = await device.GetAsync<bool>("Paired");
            logger.LogInformation("Pairing with device {Address} {Result}", address, isPaired ? "succeeded" : "failed");

            return isPaired;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to pair with device {Address}", address);
            return false;
        }
    }

    public async Task<bool> ConnectDeviceAsync(string address, CancellationToken cancellationToken = default)
    {
        if (_adapter == null)
            throw new InvalidOperationException("Adapter not initialized");

        try
        {
            var devices = await _adapter.GetDevicesAsync();
            var device = devices.FirstOrDefault(d => d.GetAsync<string>("Address").GetAwaiter().GetResult() == address);

            if (device == null)
            {
                logger.LogWarning("Device {Address} not found for connection", address);
                return false;
            }

            if (await device.GetAsync<bool>("Connected"))
            {
                logger.LogInformation("Device {Address} is already connected", address);
                return true;
            }

            logger.LogInformation("Connecting to device {Address}...", address);
            await device.ConnectAsync();

            var isConnected = await device.GetAsync<bool>("Connected");
            logger.LogInformation("Connection to device {Address} {Result}", address, isConnected ? "succeeded" : "failed");

            if (isConnected)
            {
                _devices[address] = device;
                DeviceConnected?.Invoke(this, new BluetoothDeviceEventArgs(await CreateDeviceInfoAsync(device)));
            }

            return isConnected;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to connect to device {Address}", address);
            return false;
        }
    }

    public async Task<bool> DisconnectDeviceAsync(string address, CancellationToken cancellationToken = default)
    {
        if (_adapter == null)
            throw new InvalidOperationException("Adapter not initialized");

        try
        {
            var devices = await _adapter.GetDevicesAsync();
            var device = devices.FirstOrDefault(d => d.GetAsync<string>("Address").GetAwaiter().GetResult() == address);

            if (device == null)
            {
                logger.LogWarning("Device {Address} not found for disconnection", address);
                return false;
            }

            if (!await device.GetAsync<bool>("Connected"))
            {
                logger.LogInformation("Device {Address} is already disconnected", address);
                return true;
            }

            logger.LogInformation("Disconnecting from device {Address}...", address);
            await device.DisconnectAsync();

            var isConnected = await device.GetAsync<bool>("Connected");
            logger.LogInformation("Disconnection from device {Address} {Result}", address, !isConnected ? "succeeded" : "failed");

            if (!isConnected)
            {
                _devices.Remove(address);
                DeviceDisconnected?.Invoke(this, new BluetoothDeviceEventArgs(await CreateDeviceInfoAsync(device)));
            }

            return !isConnected;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to disconnect from device {Address}", address);
            return false;
        }
    }

    private async Task PollForDevicesAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Started polling for Bluetooth devices");

        while (!cancellationToken.IsCancellationRequested && IsDiscovering && _adapter != null)
        {
            try
            {
                var devices = await _adapter.GetDevicesAsync();
                foreach (var device in devices)
                {
                    var address = await device.GetAsync<string>("Address");

                    // Check if this is a newly discovered device
                    if (!_discoveredDevices.Contains(address))
                    {
                        _discoveredDevices.Add(address);

                        try
                        {
                            var deviceInfo = await CreateDeviceInfoAsync(device);
                            logger.LogInformation("Device found: {Name} ({Address}) - Connected: {Connected}, Paired: {Paired}, UUIDs: {UUIDs}",
                                deviceInfo.Name,
                                deviceInfo.Address,
                                deviceInfo.IsConnected,
                                deviceInfo.IsPaired,
                                string.Join(", ", deviceInfo.UuiDs.Take(3)));
                            DeviceFound?.Invoke(this, new BluetoothDeviceEventArgs(deviceInfo));
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Error processing device {Address}", address);
                        }
                    }
                }

                // Poll every 2 seconds
                await Task.Delay(2000, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Device polling cancelled");
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during device polling");
                await Task.Delay(5000, cancellationToken); // Wait longer on error
            }
        }

        logger.LogInformation("Stopped polling for Bluetooth devices");
    }

    private async Task<BluetoothDeviceInfo> CreateDeviceInfoAsync(IDevice1 device)
    {
        var properties = await device.GetAllAsync();
        return new BluetoothDeviceInfo
        {
            Address = properties.Address,
            Name = properties.Name ?? "Unknown Device",
            Alias = properties.Alias,
            IsConnected = properties.Connected,
            IsPaired = properties.Paired,
            IsTrusted = properties.Trusted,
            IsBlocked = properties.Blocked,
            Rssi = properties.RSSI,
            TxPower = properties.TxPower,
            Class = properties.Class,
            UuiDs = properties.UUIDs ?? []
        };
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        try
        {
            if (_adapter != null)
            {
                if (IsDiscovering)
                {
                    _ = Task.Run(async () => await StopDiscoveryAsync());
                }
            }

            _devices.Clear();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during disposal");
        }
        finally
        {
            _disposed = true;
        }
    }
}