using FastEndpoints;
using Zapper.API.Models.Requests;
using Zapper.Services;

namespace Zapper.Endpoints.Devices;

public class DeleteDeviceEndpoint(IDeviceService deviceService) : Endpoint<DeleteDeviceRequest>
{
    private readonly IDeviceService _deviceService = deviceService;

    public override void Configure()
    {
        Delete("/api/devices/{id}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Delete a device";
            s.Description = "Delete an existing device configuration";
        });
    }

    public override async Task HandleAsync(DeleteDeviceRequest req, CancellationToken ct)
    {
        var success = await _deviceService.DeleteDeviceAsync(req.Id);
        if (!success)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        await SendNoContentAsync(ct);
    }
}