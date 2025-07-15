namespace Zapper.API.Endpoints.Devices;

public class XboxDeviceDto
{
    public string Name { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string LiveId { get; set; } = string.Empty;
    public string ConsoleType { get; set; } = string.Empty;
    public bool IsAuthenticated { get; set; }
}