namespace Zapper.Device.Denon;

public interface IDenonDiscovery
{
    Task<IEnumerable<DenonDevice>> DiscoverDevicesAsync(CancellationToken cancellationToken = default);
}

public record DenonDevice(string IpAddress, string Model, string Name, string SerialNumber);