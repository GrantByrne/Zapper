namespace Zapper.API.Models.Responses;

public class PairWebOSDeviceResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ClientKey { get; set; }
}