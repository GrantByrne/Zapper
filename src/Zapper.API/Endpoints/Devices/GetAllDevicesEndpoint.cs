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
            s.Description = "Retrieve a list of all configured devices";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var devices = await deviceService.GetAllDevicesAsync();
        await SendOkAsync(devices, ct);
    }
}