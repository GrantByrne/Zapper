using Zapper.Core.Models;

namespace Zapper.API.Models.Requests;

public class CreateDeviceRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public DeviceType Type { get; set; }
    public ConnectionType ConnectionType { get; set; }
    public string? IpAddress { get; set; }
    public int? Port { get; set; }
    public string? MacAddress { get; set; }
    public string? AuthenticationToken { get; set; }
}