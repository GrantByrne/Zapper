using Zapper.Core.Models;

namespace Zapper.Hardware;

public interface IWebOSDeviceController
{
    Task<bool> SendCommandAsync(Device device, DeviceCommand command, CancellationToken cancellationToken = default);
    Task<bool> TestConnectionAsync(Device device, CancellationToken cancellationToken = default);
}