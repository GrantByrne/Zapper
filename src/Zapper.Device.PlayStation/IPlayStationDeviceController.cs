namespace Zapper.Device.PlayStation;

public interface IPlayStationDeviceController
{
    Task<bool> Connect(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default);
    Task<bool> Disconnect(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default);
    Task<bool> SendCommand(Zapper.Core.Models.Device device, Core.Models.DeviceCommand command, CancellationToken cancellationToken = default);
    Task<bool> TestConnection(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default);
    Task<bool> PowerOn(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default);
    Task<bool> PowerOff(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default);
    Task<bool> Navigate(Zapper.Core.Models.Device device, string direction, CancellationToken cancellationToken = default);
}