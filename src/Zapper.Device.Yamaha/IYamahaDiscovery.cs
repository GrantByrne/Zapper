using Zapper.Core.Models;

namespace Zapper.Device.Yamaha;

public interface IYamahaDiscovery
{
    event EventHandler<Zapper.Core.Models.Device>? DeviceDiscovered;
    Task<IEnumerable<Zapper.Core.Models.Device>> DiscoverDevicesAsync(TimeSpan timeout = default, CancellationToken cancellationToken = default);
}