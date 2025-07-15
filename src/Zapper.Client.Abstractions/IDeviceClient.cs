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

public class WebOSScanRequest
{
    public int DurationSeconds { get; set; } = 15;
}

public class WebOSScanResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public bool IsScanning { get; set; }
}