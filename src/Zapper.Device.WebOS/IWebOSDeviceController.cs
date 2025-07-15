using Zapper.Core.Models;

namespace Zapper.Device.WebOS;

public interface IWebOsDeviceController
{
    Task<bool> SendCommand(Zapper.Core.Models.Device device, DeviceCommand command, CancellationToken cancellationToken = default);
    Task<bool> TestConnection(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default);
}