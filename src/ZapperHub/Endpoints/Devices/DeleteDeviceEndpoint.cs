using FastEndpoints;
using ZapperHub.Services;

namespace ZapperHub.Endpoints.Devices;

public class DeleteDeviceRequest
{
    public int Id { get; set; }
}

public class DeleteDeviceEndpoint : Endpoint<DeleteDeviceRequest>
{
    public IDeviceService DeviceService { get; set; } = null!;

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
        var success = await DeviceService.DeleteDeviceAsync(req.Id);
        if (!success)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        await SendNoContentAsync(ct);
    }
}