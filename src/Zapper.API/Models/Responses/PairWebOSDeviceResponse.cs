namespace Zapper.API.Models.Responses;

public class PairWebOsDeviceResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public string? ClientKey { get; set; }
}