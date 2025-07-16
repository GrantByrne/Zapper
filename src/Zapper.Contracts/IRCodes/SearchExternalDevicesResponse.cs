namespace Zapper.Contracts.IRCodes;

public class SearchExternalDevicesResponse
{
    public IEnumerable<ExternalDeviceInfo> Devices { get; set; } = [];
}