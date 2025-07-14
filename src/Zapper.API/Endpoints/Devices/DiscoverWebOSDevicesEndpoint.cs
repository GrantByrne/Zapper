using FastEndpoints;
using Zapper.API.Models.Requests;
using Zapper.Device.WebOS;

namespace Zapper.Endpoints.Devices;

public class DiscoverWebOsDevicesEndpoint(IWebOsDiscovery webOsDiscovery) : Endpoint<DiscoverWebOsDevicesRequest, IEnumerable<Zapper.Core.Models.Device>>
{

    public override void Configure()
    {
        Post("/api/devices/discover/webos");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Discover WebOS devices on the network";
            s.Description = "Scan the network for LG WebOS smart TVs using SSDP and mDNS discovery";
        });
    }

    public override async Task HandleAsync(DiscoverWebOsDevicesRequest req, CancellationToken ct)
    {
        var timeout = TimeSpan.FromSeconds(Math.Max(1, Math.Min(req.TimeoutSeconds, 60)));
        var devices = await webOsDiscovery.DiscoverDevicesAsync(timeout, ct);
        await SendOkAsync(devices, ct);
    }
}