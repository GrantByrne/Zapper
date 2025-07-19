using Zapper.Device.AndroidTV.Models;

namespace Zapper.Device.AndroidTV;

public interface IAdbDiscoveryService
{
    Task<IEnumerable<AdbDevice>> DiscoverDevicesAsync(CancellationToken cancellationToken = default);
    Task<bool> TestDeviceAsync(string host, int port = 5555, CancellationToken cancellationToken = default);
}