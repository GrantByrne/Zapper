namespace Zapper.Client.IRCodes;

public class ExternalDeviceInfo
{
    public required string Manufacturer { get; set; }
    public required string DeviceType { get; set; }
    public required string Device { get; set; }
    public required string Subdevice { get; set; }
}