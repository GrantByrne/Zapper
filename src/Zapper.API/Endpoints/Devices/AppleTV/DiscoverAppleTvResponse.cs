using Zapper.Device.AppleTV.Models;

namespace Zapper.API.Endpoints.Devices.AppleTV;

public class DiscoverAppleTvResponse
{
    public List<AppleTvDevice> Devices { get; set; } = new();
}