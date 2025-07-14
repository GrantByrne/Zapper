using Zapper.Core.Models;

namespace Zapper.Device.Roku;

public interface IRokuDeviceController
{
    Task<bool> SendCommandAsync(Zapper.Core.Models.Device device, DeviceCommand command, CancellationToken cancellationToken = default);
    Task<bool> TestConnectionAsync(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default);
    Task<string?> GetDeviceInfoAsync(string ipAddress, CancellationToken cancellationToken = default);
    Task<bool> LaunchAppAsync(string ipAddress, string appId, CancellationToken cancellationToken = default);
    Task<bool> SendKeyAsync(string ipAddress, string keyCode, CancellationToken cancellationToken = default);
}