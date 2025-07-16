namespace Zapper.Client.UsbRemotes;

public record GetButtonMappingsRequest
{
    public int RemoteId { get; init; }
}