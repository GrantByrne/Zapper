namespace Zapper.Contracts.Devices;

public class RokuDeviceDto
{
    public string Name { get; set; } = "";
    public string IpAddress { get; set; } = "";
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public int Port { get; set; } = 8060;
}