using Zapper.Core.Models;

namespace Zapper.Device.Tizen;

public interface ITizenDiscovery
{
    Task<IEnumerable<Zapper.Core.Models.Device>> DiscoverDevices(TimeSpan timeout = default, CancellationToken cancellationToken = default);
    Task<Zapper.Core.Models.Device?> DiscoverDeviceByIp(string ipAddress, CancellationToken cancellationToken = default);
    Task<bool> PairWithDevice(Zapper.Core.Models.Device device, string? pinCode = null, CancellationToken cancellationToken = default);
    event EventHandler<Zapper.Core.Models.Device>? DeviceDiscovered;
}