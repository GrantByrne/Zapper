namespace Zapper.Client.Devices;

public class PairTizenDeviceResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public string? AuthToken { get; set; }
}