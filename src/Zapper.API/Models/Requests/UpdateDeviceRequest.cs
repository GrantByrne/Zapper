using Zapper.Core.Models;

namespace Zapper.API.Models.Requests;

public class UpdateDeviceRequest
{
    public int Id { get; set; }
    public Device Device { get; set; } = null!;
}