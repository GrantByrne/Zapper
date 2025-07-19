namespace Zapper.Services;

public class RemoteStatus
{
    public bool IsAdvertising { get; set; }
    public string? RemoteName { get; set; }
    public RemoteDeviceType DeviceType { get; set; }
    public string? ConnectedHostAddress { get; set; }
    public string? ConnectedHostName { get; set; }
    public DateTime? ConnectedAt { get; set; }
}