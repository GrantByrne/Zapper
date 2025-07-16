using FastEndpoints;
using Zapper.Client.Devices;
using Zapper.Services;

namespace Zapper.API.Endpoints.Devices;

public class GetDeviceEndpoint(IDeviceService deviceService) : Endpoint<GetDeviceRequest, Zapper.Core.Models.Device>
{

    public override void Configure()
    {
        Get("/api/devices/{id}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get device by ID";
            s.Description = "Retrieve a specific device by its unique identifier, including all configuration details and capabilities.";
            s.Responses[200] = "Device found and returned successfully";
            s.Responses[404] = "Device not found with the specified ID";
            s.Responses[500] = "Internal server error";
        });
        Tags("Devices");
    }

    public override async Task HandleAsync(GetDeviceRequest req, CancellationToken ct)
    {
        var device = await deviceService.GetDevice(req.Id);
        if (device == null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        await SendOkAsync(device, ct);
    }
}