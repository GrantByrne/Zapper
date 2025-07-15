namespace Zapper.Contracts.Devices;

public class StopBluetoothScanResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
}

public class StopWebOSScanResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
}