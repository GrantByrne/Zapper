using FastEndpoints;
using Zapper.Services;

namespace Zapper.API.Endpoints.Devices;

public class GetAllDevicesEndpoint(IDeviceService deviceService) : EndpointWithoutRequest<IEnumerable<Zapper.Core.Models.Device>>
{

    public override void Configure()
    {
        Get("/api/devices");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get all devices";
            s.Description = "Retrieve a list of all configured devices in the system. Devices can include TVs, receivers, streaming devices, and other controllable equipment.";
            s.Responses[200] = "List of devices retrieved successfully";
            s.Responses[500] = "Internal server error";
        });
        Tags("Devices");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var devices = await deviceService.GetAllDevices();
        await SendOkAsync(devices, ct);
    }
}