namespace Zapper.Contracts.IRCodes;

public class SearchExternalDevicesRequest
{
    public string? Manufacturer { get; set; }
    public string? DeviceType { get; set; }
}