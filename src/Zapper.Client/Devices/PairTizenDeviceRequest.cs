namespace Zapper.Client.Devices;

public record PairTizenDeviceRequest
{
    public int DeviceId { get; init; }
    public string? PinCode { get; init; }
}