using Zapper.Client.Devices;

namespace Zapper.Services;

public interface IDeviceService
{
    Task<IEnumerable<Zapper.Core.Models.Device>> GetAllDevices();
    Task<Zapper.Core.Models.Device?> GetDevice(int id);
    Task<Zapper.Core.Models.Device> CreateDevice(Zapper.Core.Models.Device device);
    Task<Zapper.Core.Models.Device?> UpdateDevice(int id, Zapper.Core.Models.Device device);
    Task<bool> DeleteDevice(int id);
    Task<bool> SendCommand(int deviceId, string commandName, CancellationToken cancellationToken = default);
    Task<bool> SendCommand(int deviceId, SendCommandRequest request, CancellationToken cancellationToken = default);
    Task<bool> TestDeviceConnection(int deviceId);
    Task<IEnumerable<Zapper.Core.Models.Device>> DiscoverDevices(string deviceType, CancellationToken cancellationToken = default);
}