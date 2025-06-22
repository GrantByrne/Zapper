using FastEndpoints;
using Zapper.API.Models.Requests;
using Zapper.Core.Models;
using Zapper.Services;

namespace Zapper.Endpoints.Devices;

public class GetDeviceEndpoint(IDeviceService deviceService) : Endpoint<GetDeviceRequest, Device>
{

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
        var device = await deviceService.GetDeviceAsync(req.Id);
        if (device == null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        await SendOkAsync(device, ct);
    }
}