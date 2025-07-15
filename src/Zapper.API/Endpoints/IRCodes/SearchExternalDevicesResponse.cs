namespace Zapper.API.Endpoints.IRCodes;

public class SearchExternalDevicesResponse
{
    public IEnumerable<ExternalDeviceInfo> Devices { get; set; } = [];
}