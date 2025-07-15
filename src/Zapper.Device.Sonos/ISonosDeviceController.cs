namespace Zapper.Device.Sonos;

public interface ISonosDeviceController
{
    Task<bool> ConnectAsync(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default);
    Task<bool> DisconnectAsync(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default);
    Task<bool> SendCommandAsync(Zapper.Core.Models.Device device, Zapper.Core.Models.DeviceCommand command, CancellationToken cancellationToken = default);
    Task<bool> TestConnectionAsync(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default);
    Task<bool> PowerOnAsync(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default);
    Task<bool> PowerOffAsync(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default);
    Task<bool> SetVolumeAsync(Zapper.Core.Models.Device device, int volume, CancellationToken cancellationToken = default);
    Task<bool> PlayAsync(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default);
    Task<bool> PauseAsync(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default);
    Task<bool> StopAsync(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default);
}