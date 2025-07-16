namespace Zapper.Contracts.IRCodes;

public class LearnIrCommandRequest
{
    public string CommandName { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;
}