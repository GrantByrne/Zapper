namespace Zapper.API.Endpoints.Devices;

public class XboxDeviceDto
{
    public string Name { get; set; } = "";
    public string IpAddress { get; set; } = "";
    public string LiveId { get; set; } = "";
    public string ConsoleType { get; set; } = "";
    public bool IsAuthenticated { get; set; }
}