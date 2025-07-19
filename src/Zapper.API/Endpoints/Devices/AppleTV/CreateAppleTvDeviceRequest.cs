using Zapper.Device.AppleTV.Models;

namespace Zapper.API.Endpoints.Devices.AppleTV;

public class CreateAppleTvDeviceRequest
{
    public required AppleTvDevice DiscoveredDevice { get; set; }
}