using Zapper.Core.Models;

namespace Zapper.Device.AppleTV.Models;

public class AppleTvDevice
{
    public string Name { get; set; } = "";
    public string Identifier { get; set; } = "";
    public string Model { get; set; } = "";
    public string OperatingSystem { get; set; } = "";
    public string Version { get; set; } = "";
    public string IpAddress { get; set; } = "";
    public int Port { get; set; }
    public List<string> Services { get; set; } = new();
    public Dictionary<string, string> Properties { get; set; } = new();
    public bool RequiresPairing { get; set; }
    public bool IsPaired { get; set; }
    public ConnectionType PreferredProtocol { get; set; }
}