namespace Zapper.Client.Devices;

public class PairWebOsDeviceResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public string? ClientKey { get; set; }
}