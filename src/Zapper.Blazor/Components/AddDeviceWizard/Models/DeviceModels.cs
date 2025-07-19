namespace Zapper.Blazor.Components.AddDeviceWizard.Models;

public class PlayStationDevice
{
    public string Name { get; set; } = "";
    public string IpAddress { get; set; } = "";
    public string Model { get; set; } = "";
}

public class XboxDevice
{
    public string Name { get; set; } = "";
    public string IpAddress { get; set; } = "";
    public string LiveId { get; set; } = "";
    public string ConsoleType { get; set; } = "";
    public bool IsAuthenticated { get; set; }
}

public class RokuDevice
{
    public string Name { get; set; } = "";
    public string IpAddress { get; set; } = "";
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public int Port { get; set; } = 8060;
}

public class YamahaDevice
{
    public string Name { get; set; } = "";
    public string IpAddress { get; set; } = "";
    public string? Model { get; set; }
    public string? Zone { get; set; }
    public string? Version { get; set; }
}

public class SonosDevice
{
    public string Name { get; set; } = "";
    public string IpAddress { get; set; } = "";
    public string? Model { get; set; }
    public string? Zone { get; set; }
    public string? RoomName { get; set; }
    public string? SerialNumber { get; set; }
}

public class DenonDevice
{
    public string Name { get; set; } = "";
    public string IpAddress { get; set; } = "";
    public string Model { get; set; } = "";
    public string SerialNumber { get; set; } = "";
}