using FastEndpoints;
using Zapper.Models;
using Zapper.Services;

namespace Zapper.Endpoints.Devices;

public class CreateDeviceEndpoint : Endpoint<Device, Device>
{
    public IDeviceService DeviceService { get; set; } = null!;

    public override void Configure()
    {
        Post("/api/devices");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Create a new device";
            s.Description = "Create a new device configuration";
        });
    }

    public override async Task HandleAsync(Device req, CancellationToken ct)
    {
        var createdDevice = await DeviceService.CreateDeviceAsync(req);
        await SendCreatedAtAsync<GetDeviceEndpoint>(new { id = createdDevice.Id }, createdDevice, cancellation: ct);
    }
}