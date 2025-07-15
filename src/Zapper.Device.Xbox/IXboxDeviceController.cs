
namespace Zapper.Device.Xbox;

public interface IXboxDeviceController
{
    Task<bool> Connect(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default);
    Task<bool> Disconnect(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default);
    Task<bool> SendCommand(Zapper.Core.Models.Device device, Zapper.Core.Models.DeviceCommand command, CancellationToken cancellationToken = default);
    Task<bool> TestConnection(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default);
    Task<bool> PowerOn(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default);
    Task<bool> PowerOff(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default);
    Task<bool> SendText(Zapper.Core.Models.Device device, string text, CancellationToken cancellationToken = default);
}