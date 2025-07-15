namespace Zapper.Contracts.Devices;

/// <summary>
/// Request to discover PlayStation devices on the network
/// </summary>
public class DiscoverPlayStationDevicesRequest
{
    /// <summary>
    /// Timeout in seconds for the discovery operation (default: 10, max: 60)
    /// </summary>
    public int TimeoutSeconds { get; set; } = 10;
}