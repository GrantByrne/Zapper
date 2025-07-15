namespace Zapper.Device.Xbox.Models;

public class XboxDevice
{
    public string Name { get; set; } = "";
    public string IpAddress { get; set; } = "";
    public string LiveId { get; set; } = "";
    public string Certificate { get; set; } = "";
    public XboxConsoleType ConsoleType { get; set; }
    public bool IsAuthenticated { get; set; }
    public DateTime LastSeen { get; set; }
}