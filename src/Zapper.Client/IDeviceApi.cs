using Refit;
using Zapper.Contracts;
using Zapper.Contracts.Devices;

namespace Zapper.Client;

/// <summary>
/// Refit interface for device API endpoints
/// </summary>
public interface IDeviceApi
{
    /// <summary>
    /// Get all devices
    /// </summary>
    [Get(ApiRoutes.Devices.GetAll)]
    Task<IEnumerable<DeviceDto>> GetAllDevicesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get a device by ID
    /// </summary>
    [Get(ApiRoutes.Devices.GetById)]
    Task<DeviceDto> GetDeviceAsync(int id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Create a new device
    /// </summary>
    [Post(ApiRoutes.Devices.Create)]
    Task<DeviceDto> CreateDeviceAsync([Body] CreateDeviceRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Update an existing device
    /// </summary>
    [Put(ApiRoutes.Devices.Update)]
    Task<DeviceDto> UpdateDeviceAsync(int id, [Body] UpdateDeviceRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Delete a device
    /// </summary>
    [Delete(ApiRoutes.Devices.Delete)]
    Task DeleteDeviceAsync(int id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Send a command to a device
    /// </summary>
    [Post(ApiRoutes.Devices.SendCommand)]
    Task SendCommandAsync(int id, [Body] SendCommandRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Discover available Bluetooth devices
    /// </summary>
    [Get(ApiRoutes.Devices.BluetoothDiscovery)]
    Task<IEnumerable<string>> DiscoverBluetoothDevicesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Start Bluetooth device scanning with real-time updates
    /// </summary>
    [Post(ApiRoutes.Devices.BluetoothScan)]
    Task<BluetoothScanResponse> StartBluetoothScanAsync([Body] BluetoothScanRequest request, CancellationToken cancellationToken = default);
}

public class BluetoothScanRequest
{
    public int DurationSeconds { get; set; } = 30;
}

public class BluetoothScanResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public bool IsScanning { get; set; }
}