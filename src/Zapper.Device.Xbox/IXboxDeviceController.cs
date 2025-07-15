
namespace Zapper.Device.Xbox;

public interface IXboxDeviceController
{
    Task<bool> ConnectAsync(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default);
    Task<bool> DisconnectAsync(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default);
    Task<bool> SendCommandAsync(Zapper.Core.Models.Device device, Zapper.Core.Models.DeviceCommand command, CancellationToken cancellationToken = default);
    Task<bool> TestConnectionAsync(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default);
    Task<bool> PowerOnAsync(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default);
    Task<bool> PowerOffAsync(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default);
    Task<bool> SendTextAsync(Zapper.Core.Models.Device device, string text, CancellationToken cancellationToken = default);
}