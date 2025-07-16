namespace Zapper.Contracts.UsbRemotes;

public class UpdateUsbRemoteRequest
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public bool IsActive { get; set; }
    public bool InterceptSystemButtons { get; set; }
    public int LongPressTimeoutMs { get; set; }
}