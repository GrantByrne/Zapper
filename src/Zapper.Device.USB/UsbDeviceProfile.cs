namespace Zapper.Device.USB;

public class UsbDeviceProfile
{
    public string Name { get; set; } = string.Empty;
    public int VendorId { get; set; }
    public int ProductId { get; set; }
    public Dictionary<byte, string> KeyMapping { get; set; } = new();
    public string? ProductNamePattern { get; set; }
}