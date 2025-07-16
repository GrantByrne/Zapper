namespace Zapper.Contracts.Devices;

public class SonosDeviceDto
{
    public string Name { get; set; } = "";
    public string IpAddress { get; set; } = "";
    public string? Model { get; set; }
    public string? Zone { get; set; }
    public string? RoomName { get; set; }
    public string? SerialNumber { get; set; }
}