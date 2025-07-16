namespace Zapper.Client.Devices;

public class DiscoverXboxDevicesResponse
{
    public bool Success { get; set; }
    public List<XboxDeviceDto> Devices { get; set; } = new();
}