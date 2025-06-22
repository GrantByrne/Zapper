using Zapper.Models;

namespace Zapper.Services;

public interface IDeviceService
{
    Task<IEnumerable<Device>> GetAllDevicesAsync();
    Task<Device?> GetDeviceAsync(int id);
    Task<Device> CreateDeviceAsync(Device device);
    Task<Device?> UpdateDeviceAsync(int id, Device device);
    Task<bool> DeleteDeviceAsync(int id);
    Task<bool> SendCommandAsync(int deviceId, string commandName, CancellationToken cancellationToken = default);
    Task<bool> TestDeviceConnectionAsync(int deviceId);
    Task<IEnumerable<Device>> DiscoverDevicesAsync(string deviceType, CancellationToken cancellationToken = default);
}