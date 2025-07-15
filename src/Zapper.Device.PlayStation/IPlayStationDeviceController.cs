namespace Zapper.Device.PlayStation;

public interface IPlayStationDeviceController
{
    Task<bool> ConnectAsync(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default);
    Task<bool> DisconnectAsync(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default);
    Task<bool> SendCommandAsync(Zapper.Core.Models.Device device, Core.Models.DeviceCommand command, CancellationToken cancellationToken = default);
    Task<bool> TestConnectionAsync(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default);
    Task<bool> PowerOnAsync(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default);
    Task<bool> PowerOffAsync(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default);
    Task<bool> NavigateAsync(Zapper.Core.Models.Device device, string direction, CancellationToken cancellationToken = default);
}