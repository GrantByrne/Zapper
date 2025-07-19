using FastEndpoints;
using Zapper.Client.Devices;
using Zapper.Device.Tizen;

namespace Zapper.API.Endpoints.Devices.Tizen;

public class DiscoverTizenDevicesEndpoint(ITizenDiscovery tizenDiscovery) : Endpoint<DiscoverTizenDevicesRequest, IEnumerable<Zapper.Core.Models.Device>>
{
    public override void Configure()
    {
        Post("/api/devices/discover/tizen");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Discover Samsung Tizen devices on the network";
            s.Description = "Scan the network for Samsung Tizen smart TVs using SSDP discovery. This will find all Samsung smart TVs on the local network.";
            s.ExampleRequest = new DiscoverTizenDevicesRequest { TimeoutSeconds = 10 };
            s.Responses[200] = "List of discovered Samsung Tizen devices";
            s.Responses[500] = "Internal server error during discovery";
        });
        Tags("Devices", "Discovery");
    }

    public override async Task HandleAsync(DiscoverTizenDevicesRequest req, CancellationToken ct)
    {
        var timeout = TimeSpan.FromSeconds(Math.Max(1, Math.Min(req.TimeoutSeconds, 60)));
        var devices = await tizenDiscovery.DiscoverDevices(timeout, ct);
        await SendOkAsync(devices, ct);
    }
}