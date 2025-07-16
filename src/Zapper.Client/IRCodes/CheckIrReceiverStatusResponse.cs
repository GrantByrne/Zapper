namespace Zapper.Client.IRCodes;

public class CheckIrReceiverStatusResponse
{
    public bool IsAvailable { get; set; }
    public string Message { get; set; } = "";
}