using Zapper.Core.Models;

namespace Zapper.Device.WebOS;

public interface IWebOsDeviceController
{
    Task<bool> SendCommandAsync(Zapper.Core.Models.Device device, DeviceCommand command, CancellationToken cancellationToken = default);
    Task<bool> TestConnectionAsync(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default);
}