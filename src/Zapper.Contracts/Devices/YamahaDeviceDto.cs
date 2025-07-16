namespace Zapper.Contracts.Devices;

public class YamahaDeviceDto
{
    public string Name { get; set; } = "";
    public string IpAddress { get; set; } = "";
    public string? Model { get; set; }
    public string? Zone { get; set; }
    public string? Version { get; set; }
}