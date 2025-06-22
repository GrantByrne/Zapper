using Zapper.Core.Models;

namespace Zapper.API.Models.Requests;

public class SearchIRCodeSetsRequest
{
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public DeviceType? DeviceType { get; set; }
}