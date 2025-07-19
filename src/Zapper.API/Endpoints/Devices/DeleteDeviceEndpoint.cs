using FastEndpoints;
using Zapper.Client.Devices;
using Zapper.Services;

namespace Zapper.API.Endpoints.Devices;

public class DeleteDeviceEndpoint(IDeviceService deviceService) : Endpoint<DeleteDeviceRequest>
{

    public override void Configure()
    {
        Delete("/api/devices/{id}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Delete a device";
            s.Description = "Permanently delete a device configuration from the system. This will also remove the device from any activities using it.";
            s.Responses[204] = "Device deleted successfully";
            s.Responses[404] = "Device not found";
            s.Responses[500] = "Internal server error";
        });
        Tags("Devices");
    }

    public override async Task HandleAsync(DeleteDeviceRequest req, CancellationToken ct)
    {
        var success = await deviceService.DeleteDevice(req.Id);
        if (!success)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        await SendNoContentAsync(ct);
    }
}