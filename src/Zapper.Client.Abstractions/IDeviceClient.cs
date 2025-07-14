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
}