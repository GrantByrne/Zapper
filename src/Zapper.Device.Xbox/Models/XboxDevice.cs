namespace Zapper.Device.Xbox.Models;

public class XboxDevice
{
    public string Name { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string LiveId { get; set; } = string.Empty;
    public string Certificate { get; set; } = string.Empty;
    public XboxConsoleType ConsoleType { get; set; }
    public bool IsAuthenticated { get; set; }
    public DateTime LastSeen { get; set; }
}

public enum XboxConsoleType
{
    Unknown,
    XboxOne,
    XboxOneS,
    XboxOneX,
    XboxSeriesS,
    XboxSeriesX
}