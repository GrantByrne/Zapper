namespace Zapper.Device.WebOS;

public interface IWebOSDiscovery
{
    Task<IEnumerable<Zapper.Core.Models.Device>> DiscoverDevicesAsync(TimeSpan timeout = default, CancellationToken cancellationToken = default);
    Task<Zapper.Core.Models.Device?> DiscoverDeviceByIpAsync(string ipAddress, CancellationToken cancellationToken = default);
    Task<bool> PairWithDeviceAsync(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default);
    event EventHandler<Zapper.Core.Models.Device>? DeviceDiscovered;
}