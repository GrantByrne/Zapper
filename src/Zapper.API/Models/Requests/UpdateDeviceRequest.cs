namespace Zapper.API.Models.Requests;

public class UpdateDeviceRequest
{
    public int Id { get; set; }
    public Zapper.Core.Models.Device Device { get; set; } = null!;
}