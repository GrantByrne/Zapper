namespace Zapper.Client.Remotes;

public class BluetoothRemoteStatusResponse
{
    public bool IsAdvertising { get; set; }
    public string? RemoteName { get; set; }
    public string? DeviceType { get; set; }
    public string? ConnectedHostAddress { get; set; }
    public string? ConnectedHostName { get; set; }
    public DateTime? ConnectedAt { get; set; }
}