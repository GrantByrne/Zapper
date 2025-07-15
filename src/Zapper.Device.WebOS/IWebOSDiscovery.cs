namespace Zapper.Device.WebOS;

public interface IWebOsDiscovery
{
    Task<IEnumerable<Zapper.Core.Models.Device>> DiscoverDevices(TimeSpan timeout = default, CancellationToken cancellationToken = default);
    Task<Zapper.Core.Models.Device?> DiscoverDeviceByIp(string ipAddress, CancellationToken cancellationToken = default);
    Task<bool> PairWithDevice(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default);
    event EventHandler<Zapper.Core.Models.Device>? DeviceDiscovered;
}