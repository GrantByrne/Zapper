namespace Zapper.Device.Yamaha;

public interface IYamahaDeviceController
{
    Task<bool> Connect(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default);
    Task<bool> Disconnect(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default);
    Task<bool> SendCommand(Zapper.Core.Models.Device device, Core.Models.DeviceCommand command, CancellationToken cancellationToken = default);
    Task<bool> TestConnection(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default);
    Task<bool> PowerOn(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default);
    Task<bool> PowerOff(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default);
    Task<bool> SetVolume(Zapper.Core.Models.Device device, int volume, CancellationToken cancellationToken = default);
    Task<bool> SetInput(Zapper.Core.Models.Device device, string input, CancellationToken cancellationToken = default);
    Task<bool> Mute(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default);
}