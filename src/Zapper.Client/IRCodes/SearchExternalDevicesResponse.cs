namespace Zapper.Client.IRCodes;

public class SearchExternalDevicesResponse
{
    public IEnumerable<ExternalDeviceInfo> Devices { get; set; } = [];
}