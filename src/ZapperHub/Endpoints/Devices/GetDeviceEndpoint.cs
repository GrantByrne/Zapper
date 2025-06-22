using FastEndpoints;
using ZapperHub.Models;
using ZapperHub.Services;

namespace ZapperHub.Endpoints.Devices;

public class GetDeviceRequest
{
    public int Id { get; set; }
}

public class GetDeviceEndpoint : Endpoint<GetDeviceRequest, Device>
{
    public IDeviceService DeviceService { get; set; } = null!;

    public override void Configure()
    {
        Get("/api/devices/{id}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get device by ID";
            s.Description = "Retrieve a specific device by its ID";
        });
    }

    public override async Task HandleAsync(GetDeviceRequest req, CancellationToken ct)
    {
        var device = await DeviceService.GetDeviceAsync(req.Id);
        if (device == null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        await SendOkAsync(device, ct);
    }
}