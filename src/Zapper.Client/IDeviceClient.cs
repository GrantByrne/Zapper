using Zapper.Contracts.Devices;

namespace Zapper.Client;

/// <summary>
/// Client interface for device management operations
/// </summary>
public interface IDeviceClient
{
    /// <summary>
    /// Get all devices
    /// </summary>
    Task<IEnumerable<DeviceDto>> GetAllDevicesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a device by ID
    /// </summary>
    Task<DeviceDto?> GetDeviceAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new device
    /// </summary>
    Task<DeviceDto> CreateDeviceAsync(CreateDeviceRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing device
    /// </summary>
    Task<DeviceDto> UpdateDeviceAsync(int id, UpdateDeviceRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a device
    /// </summary>
    Task DeleteDeviceAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send a command to a device
    /// </summary>
    Task SendCommandAsync(int id, SendCommandRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Discover available Bluetooth devices
    /// </summary>
    Task<IEnumerable<string>> DiscoverBluetoothDevicesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Start Bluetooth device scanning with real-time updates
    /// </summary>
    Task<BluetoothScanResponse> StartBluetoothScanAsync(BluetoothScanRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Start WebOS TV scanning with real-time updates
    /// </summary>
    Task<WebOsScanResponse> StartWebOsScanAsync(WebOsScanRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stop Bluetooth device scanning
    /// </summary>
    Task<StopBluetoothScanResponse> StopBluetoothScanAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stop WebOS TV scanning
    /// </summary>
    Task<StopWebOsScanResponse> StopWebOsScanAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Discover available PlayStation devices on the network
    /// </summary>
    Task<IEnumerable<PlayStationDeviceDto>> DiscoverPlayStationDevicesAsync(DiscoverPlayStationDevicesRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Discover available Xbox devices on the network
    /// </summary>
    Task<DiscoverXboxDevicesResponse> DiscoverXboxDevicesAsync(DiscoverXboxDevicesRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Discover available Roku devices on the network
    /// </summary>
    Task<IEnumerable<RokuDeviceDto>> DiscoverRokuDevicesAsync(DiscoverRokuDevicesRequest request, CancellationToken cancellationToken = default);
}