using Zapper.Core.Models;

namespace Zapper.Client.Devices;

public class CreateDeviceResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Brand { get; set; } = "";
    public string Model { get; set; } = "";
    public DeviceType Type { get; set; }
    public ConnectionType ConnectionType { get; set; }
    public string? IpAddress { get; set; }
    public int? Port { get; set; }
    public string? MacAddress { get; set; }
    public string? AuthenticationToken { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastSeen { get; set; }
}