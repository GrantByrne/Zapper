using Zapper.Core.Models;

namespace Zapper.Hardware;

public interface IWebOSDiscovery
{
    Task<IEnumerable<Device>> DiscoverDevicesAsync(TimeSpan timeout = default, CancellationToken cancellationToken = default);
    Task<Device?> DiscoverDeviceByIpAsync(string ipAddress, CancellationToken cancellationToken = default);
    Task<bool> PairWithDeviceAsync(Device device, CancellationToken cancellationToken = default);
    event EventHandler<Device>? DeviceDiscovered;
}