namespace Zapper.Device.Yamaha;

public interface IYamahaDeviceController
{
    Task<bool> ConnectAsync(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default);
    Task<bool> DisconnectAsync(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default);
    Task<bool> SendCommandAsync(Zapper.Core.Models.Device device, Core.Models.DeviceCommand command, CancellationToken cancellationToken = default);
    Task<bool> TestConnectionAsync(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default);
    Task<bool> PowerOnAsync(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default);
    Task<bool> PowerOffAsync(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default);
    Task<bool> SetVolumeAsync(Zapper.Core.Models.Device device, int volume, CancellationToken cancellationToken = default);
    Task<bool> SetInputAsync(Zapper.Core.Models.Device device, string input, CancellationToken cancellationToken = default);
    Task<bool> MuteAsync(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default);
}