using Zapper.Core.Models;

namespace Zapper.Device.Roku;

public interface IRokuDeviceController
{
    Task<bool> SendCommand(Zapper.Core.Models.Device device, DeviceCommand command, CancellationToken cancellationToken = default);
    Task<bool> TestConnection(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default);
    Task<string?> GetDeviceInfo(string ipAddress, CancellationToken cancellationToken = default);
    Task<bool> LaunchApp(string ipAddress, string appId, CancellationToken cancellationToken = default);
    Task<bool> SendKey(string ipAddress, string keyCode, CancellationToken cancellationToken = default);
}