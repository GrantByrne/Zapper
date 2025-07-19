namespace Zapper.API.Endpoints.Devices.AppleTV;

public class CreateAppleTvDeviceResponse
{
    public Zapper.Core.Models.Device? Device { get; set; }
    public bool Success { get; set; }
    public string? Message { get; set; }
}