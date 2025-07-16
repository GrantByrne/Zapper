namespace Zapper.Client.System;

public class StatusResponse
{
    public string Status { get; set; } = "OK";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Version { get; set; } = "1.0.0";
    public int ConnectedClients { get; set; }
    public SystemInfo System { get; set; } = new();
}