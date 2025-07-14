using Zapper.Core.Models;

namespace Zapper.API.Models.Requests;

public class AddIrCodeRequest
{
    public int CodeSetId { get; set; }
    public IrCode Code { get; set; } = null!;
}