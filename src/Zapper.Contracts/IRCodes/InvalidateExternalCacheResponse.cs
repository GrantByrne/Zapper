namespace Zapper.Contracts.IRCodes;

public class InvalidateExternalCacheResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
}