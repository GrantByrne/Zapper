namespace Zapper.Client.IRCodes;

public class GetIrCodeRequest
{
    public int CodeSetId { get; set; }
    public string CommandName { get; set; } = "";
}