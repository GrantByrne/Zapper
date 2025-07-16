using Zapper.Core.Models;

namespace Zapper.Contracts.IRCodes;

public class AddIrCodeRequest
{
    public int CodeSetId { get; set; }
    public IrCode Code { get; set; } = null!;
}