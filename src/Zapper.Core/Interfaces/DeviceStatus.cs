namespace Zapper.Core.Interfaces;

public class DeviceStatus
{
    public bool IsOnline { get; set; }
    public string? StatusMessage { get; set; }
    public Dictionary<string, object> Properties { get; set; } = new();
}