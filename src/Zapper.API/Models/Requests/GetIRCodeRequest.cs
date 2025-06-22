namespace Zapper.API.Models.Requests;

public class GetIRCodeRequest
{
    public int CodeSetId { get; set; }
    public string CommandName { get; set; } = string.Empty;
}