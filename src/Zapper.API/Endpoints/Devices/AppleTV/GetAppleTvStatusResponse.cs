using Zapper.Device.AppleTV.Models;

namespace Zapper.API.Endpoints.Devices.AppleTV;

public class GetAppleTvStatusResponse
{
    public AppleTvStatus? Status { get; set; }
    public bool Success { get; set; }
    public string? Message { get; set; }
}