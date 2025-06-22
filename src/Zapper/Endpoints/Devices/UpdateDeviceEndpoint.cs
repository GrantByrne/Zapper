using FastEndpoints;
using Zapper.Models;
using Zapper.Services;

namespace Zapper.Endpoints.Devices;

public class UpdateDeviceRequest
{
    public int Id { get; set; }
    public Device Device { get; set; } = null!;
}

public class UpdateDeviceEndpoint : Endpoint<UpdateDeviceRequest>
{
    public IDeviceService DeviceService { get; set; } = null!;

    public override void Configure()
    {
        Put("/api/devices/{id}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Update a device";
            s.Description = "Update an existing device configuration";
        });
    }

    public override async Task HandleAsync(UpdateDeviceRequest req, CancellationToken ct)
    {
        var updatedDevice = await DeviceService.UpdateDeviceAsync(req.Id, req.Device);
        if (updatedDevice == null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        await SendNoContentAsync(ct);
    }
}