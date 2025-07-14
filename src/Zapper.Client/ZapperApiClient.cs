using Zapper.Client.Abstractions;

namespace Zapper.Client;

/// <summary>
/// Main implementation of the Zapper API client
/// </summary>
public class ZapperApiClient(IDeviceClient deviceClient) : IZapperApiClient
{
    public IDeviceClient Devices { get; } = deviceClient;
}