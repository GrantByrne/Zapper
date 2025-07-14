using Zapper.Core.Models;

namespace Zapper.Device.Contracts;

public interface IWebOSDeviceController
{
    Task<bool> SendCommandAsync(Zapper.Core.Models.Device device, DeviceCommand command, CancellationToken cancellationToken = default);
    Task<bool> TestConnectionAsync(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default);
}