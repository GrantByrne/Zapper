using Zapper.Client.Abstractions;

namespace Zapper.Client;

/// <summary>
/// Main implementation of the Zapper API client
/// </summary>
public class ZapperApiClient(IDeviceClient deviceClient, IActivityClient activityClient) : IZapperApiClient
{
    public IDeviceClient Devices { get; } = deviceClient;
    public IActivityClient Activities { get; } = activityClient;
}