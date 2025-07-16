namespace Zapper.Contracts.UsbRemotes;

public record GetButtonMappingsRequest
{
    public int RemoteId { get; init; }
}