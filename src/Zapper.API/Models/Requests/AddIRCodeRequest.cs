using Zapper.Core.Models;

namespace Zapper.API.Models.Requests;

public class AddIRCodeRequest
{
    public int CodeSetId { get; set; }
    public IRCode Code { get; set; } = null!;
}