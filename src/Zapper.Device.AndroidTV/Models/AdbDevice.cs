namespace Zapper.Device.AndroidTV.Models;

public class AdbDevice
{
    public string IpAddress { get; set; } = "";
    public int Port { get; set; } = 5555;
    public string Name { get; set; } = "";
    public string Manufacturer { get; set; } = "";
    public string AndroidVersion { get; set; } = "";
    public bool IsAndroidTv { get; set; }
    public bool IsConnected { get; set; }
    public DateTime LastSeen { get; set; } = DateTime.UtcNow;
}