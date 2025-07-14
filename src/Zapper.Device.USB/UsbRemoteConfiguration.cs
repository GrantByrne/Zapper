namespace Zapper.Device.USB;

public class UsbRemoteConfiguration
{
    public bool UseMockHandler { get; set; } = true;
    public bool EnableDebugLogging { get; set; } = false;
    public int DeviceMonitoringIntervalMs { get; set; } = 5000;
    public List<UsbDeviceProfile> SupportedDevices { get; set; } = new();
}

public class UsbDeviceProfile
{
    public string Name { get; set; } = string.Empty;
    public int VendorId { get; set; }
    public int ProductId { get; set; }
    public Dictionary<byte, string> KeyMapping { get; set; } = new();
    public string? ProductNamePattern { get; set; }
}