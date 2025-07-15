using Zapper.Contracts.Devices;

namespace Zapper.Client.Abstractions;

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
    Task<WebOSScanResponse> StartWebOSScanAsync(WebOSScanRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stop Bluetooth device scanning
    /// </summary>
    Task<StopBluetoothScanResponse> StopBluetoothScanAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stop WebOS TV scanning
    /// </summary>
    Task<StopWebOSScanResponse> StopWebOSScanAsync(CancellationToken cancellationToken = default);
}