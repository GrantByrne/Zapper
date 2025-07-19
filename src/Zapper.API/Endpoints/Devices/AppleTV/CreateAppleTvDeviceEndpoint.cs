using FastEndpoints;
using Zapper.Core.Models;
using Zapper.Device.AppleTV.Services;

namespace Zapper.API.Endpoints.Devices.AppleTV;

public class CreateAppleTvDeviceEndpoint(AppleTvService appleTvService)
    : Endpoint<CreateAppleTvDeviceRequest, CreateAppleTvDeviceResponse>
{
    public override void Configure()
    {
        Post("/api/devices/appletv/create");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Create a device from a discovered Apple TV";
            s.Description = "Creates a new device entry from a discovered Apple TV";
        });
    }

    public override async Task HandleAsync(CreateAppleTvDeviceRequest req, CancellationToken ct)
    {
        var device = await appleTvService.CreateDeviceFromDiscoveredAsync(req.DiscoveredDevice);

        if (device != null)
        {
            await SendOkAsync(new CreateAppleTvDeviceResponse
            {
                Device = device,
                Success = true,
                Message = "Apple TV device created successfully"
            }, ct);
        }
        else
        {
            await SendAsync(new CreateAppleTvDeviceResponse
            {
                Success = false,
                Message = "Failed to create Apple TV device"
            }, 400, ct);
        }
    }
}