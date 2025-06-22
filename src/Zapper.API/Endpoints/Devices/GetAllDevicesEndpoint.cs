using FastEndpoints;
using Zapper.Core.Models;
using Zapper.Services;

namespace Zapper.Endpoints.Devices;

public class GetAllDevicesEndpoint : EndpointWithoutRequest<IEnumerable<Device>>
{
    public IDeviceService DeviceService { get; set; } = null!;

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
        var devices = await DeviceService.GetAllDevicesAsync();
        await SendOkAsync(devices, ct);
    }
}