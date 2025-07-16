namespace Zapper.Contracts.IRCodes;

public class GetExternalCodeSetRequest
{
    public required string Manufacturer { get; set; }
    public required string DeviceType { get; set; }
    public required string Device { get; set; }
    public required string Subdevice { get; set; }
}