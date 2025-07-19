namespace Zapper.Client.Devices;

public record DiscoverTizenDevicesRequest
{
    public int TimeoutSeconds { get; init; } = 5;
}