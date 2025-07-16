using Zapper.Core.Models;

namespace Zapper.Client.IRCodes;

public class AddIrCodeRequest
{
    public int CodeSetId { get; set; }
    public IrCode Code { get; set; } = null!;
}