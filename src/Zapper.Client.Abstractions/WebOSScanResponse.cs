namespace Zapper.Client.Abstractions;

public class WebOsScanResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public bool IsScanning { get; set; }
}