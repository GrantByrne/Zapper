using Zapper.Core.Models;

namespace Zapper.Client.IRCodes;

public class SearchIrCodeSetsRequest
{
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public DeviceType? DeviceType { get; set; }
}