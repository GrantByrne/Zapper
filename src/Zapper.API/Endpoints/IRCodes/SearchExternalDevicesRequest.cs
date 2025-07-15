namespace Zapper.API.Endpoints.IRCodes;

public class SearchExternalDevicesRequest
{
    public string? Manufacturer { get; set; }
    public string? DeviceType { get; set; }
}