namespace Zapper.Device.Sonos;

public interface ISonosDeviceController
{
    Task<bool> Connect(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default);
    Task<bool> Disconnect(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default);
    Task<bool> SendCommand(Zapper.Core.Models.Device device, Core.Models.DeviceCommand command, CancellationToken cancellationToken = default);
    Task<bool> TestConnection(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default);
    Task<bool> PowerOn(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default);
    Task<bool> PowerOff(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default);
    Task<bool> SetVolume(Zapper.Core.Models.Device device, int volume, CancellationToken cancellationToken = default);
    Task<bool> Play(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default);
    Task<bool> Pause(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default);
    Task<bool> Stop(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default);
}