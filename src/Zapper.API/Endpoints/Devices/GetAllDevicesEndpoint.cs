using FastEndpoints;
using Zapper.Core.Models;
using Zapper.Services;

namespace Zapper.Endpoints.Devices;

public class GetAllDevicesEndpoint(IDeviceService deviceService) : EndpointWithoutRequest<IEnumerable<Device>>
{
    private readonly IDeviceService _deviceService = deviceService;

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
        var devices = await _deviceService.GetAllDevicesAsync();
        await SendOkAsync(devices, ct);
    }
}