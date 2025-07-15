namespace Zapper.API.Endpoints.Devices;

public class BluetoothScanResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public bool IsScanning { get; set; }
}