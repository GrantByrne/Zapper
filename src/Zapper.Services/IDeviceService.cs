namespace Zapper.Services;

public interface IDeviceService
{
    Task<IEnumerable<Zapper.Core.Models.Device>> GetAllDevicesAsync();
    Task<Zapper.Core.Models.Device?> GetDeviceAsync(int id);
    Task<Zapper.Core.Models.Device> CreateDeviceAsync(Zapper.Core.Models.Device device);
    Task<Zapper.Core.Models.Device?> UpdateDeviceAsync(int id, Zapper.Core.Models.Device device);
    Task<bool> DeleteDeviceAsync(int id);
    Task<bool> SendCommandAsync(int deviceId, string commandName, CancellationToken cancellationToken = default);
    Task<bool> TestDeviceConnectionAsync(int deviceId);
    Task<IEnumerable<Zapper.Core.Models.Device>> DiscoverDevicesAsync(string deviceType, CancellationToken cancellationToken = default);
}