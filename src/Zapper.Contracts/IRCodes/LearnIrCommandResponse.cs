using Zapper.Core.Models;

namespace Zapper.Contracts.IRCodes;

public class LearnIrCommandResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public IrCode? LearnedCode { get; set; }
}