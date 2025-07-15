namespace Zapper.Device.PlayStation;

public interface IPlayStationDiscovery
{
    event EventHandler<Zapper.Core.Models.Device>? DeviceDiscovered;
    Task<IEnumerable<Zapper.Core.Models.Device>> DiscoverDevices(TimeSpan timeout = default, CancellationToken cancellationToken = default);
}