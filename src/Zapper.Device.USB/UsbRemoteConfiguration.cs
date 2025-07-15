namespace Zapper.Device.USB;

public class UsbRemoteConfiguration
{
    public bool UseMockHandler { get; set; } = true;
    public bool EnableDebugLogging { get; set; } = false;
    public int DeviceMonitoringIntervalMs { get; set; } = 5000;
    public List<UsbDeviceProfile> SupportedDevices { get; set; } = new();
}