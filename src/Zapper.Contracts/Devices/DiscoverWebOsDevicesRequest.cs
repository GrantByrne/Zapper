namespace Zapper.Contracts.Devices;

public record DiscoverWebOsDevicesRequest
{
    public int TimeoutSeconds { get; init; } = 5;
}