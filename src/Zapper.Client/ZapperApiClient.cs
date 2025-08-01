// using Zapper.Client; // No need to reference own namespace

namespace Zapper.Client;

/// <summary>
/// Main implementation of the Zapper API client
/// </summary>
public class ZapperApiClient(IDeviceClient deviceClient, IActivityClient activityClient) : IZapperApiClient
{
    public IDeviceClient Devices { get; } = deviceClient;
    public IActivityClient Activities { get; } = activityClient;
}