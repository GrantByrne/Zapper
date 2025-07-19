namespace Zapper.Device.USB;

public class UsbRemoteConfiguration
{
    public bool UseMockHandler { get; set; } = true;
    public bool EnableDebugLogging { get; set; } = false;
    public int DeviceMonitoringIntervalMs { get; set; } = 5000;
    public List<UsbDeviceProfile> SupportedDevices { get; set; } = new();
    public bool AllowAllHidDevices { get; set; } = false;
    public List<int> AdditionalVendorIds { get; set; } = new();
    public List<string> AdditionalKeywords { get; set; } = new();
}