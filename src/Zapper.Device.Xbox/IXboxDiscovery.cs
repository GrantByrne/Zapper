using Zapper.Device.Xbox.Models;

namespace Zapper.Device.Xbox;

public interface IXboxDiscovery
{
    Task<IEnumerable<XboxDevice>> DiscoverDevices(TimeSpan timeout, CancellationToken cancellationToken = default);
    event EventHandler<XboxDevice>? DeviceFound;
}