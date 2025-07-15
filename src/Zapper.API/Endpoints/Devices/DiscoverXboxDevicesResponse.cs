namespace Zapper.Endpoints.Devices;

public class DiscoverXboxDevicesResponse
{
    public bool Success { get; set; }
    public List<XboxDeviceDto> Devices { get; set; } = new();
}