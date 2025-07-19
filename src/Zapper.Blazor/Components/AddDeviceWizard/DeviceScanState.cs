namespace Zapper.Blazor.Components.AddDeviceWizard;

public class DeviceScanState<TDevice> where TDevice : class
{
    public bool IsScanning { get; set; }
    public string Error { get; set; } = "";
    public List<TDevice> DiscoveredDevices { get; set; } = new();
    public TDevice? SelectedDevice { get; set; }
    public string ManualIpAddress { get; set; } = "";
}