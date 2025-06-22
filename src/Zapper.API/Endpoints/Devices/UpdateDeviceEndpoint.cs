using FastEndpoints;
using Zapper.API.Models.Requests;
using Zapper.Services;

namespace Zapper.Endpoints.Devices;

public class UpdateDeviceEndpoint(IDeviceService deviceService) : Endpoint<UpdateDeviceRequest>
{

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
        var updatedDevice = await deviceService.UpdateDeviceAsync(req.Id, req.Device);
        if (updatedDevice == null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        await SendNoContentAsync(ct);
    }
}