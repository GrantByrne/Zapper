namespace Zapper.API.Endpoints.IRCodes;

public class InvalidateExternalCacheResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}