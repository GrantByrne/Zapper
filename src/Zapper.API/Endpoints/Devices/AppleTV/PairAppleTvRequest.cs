namespace Zapper.API.Endpoints.Devices.AppleTV;

public class PairAppleTvRequest
{
    public int DeviceId { get; set; }
    public required string Pin { get; set; }
}