using FastEndpoints;
using Zapper.API.Models.Requests;
using Zapper.Core.Models;
using Zapper.Integrations;

namespace Zapper.Endpoints.Devices;

public class DiscoverWebOSDevicesEndpoint(IWebOSDiscovery webOSDiscovery) : Endpoint<DiscoverWebOSDevicesRequest, IEnumerable<Device>>
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

    public override async Task HandleAsync(DiscoverWebOSDevicesRequest req, CancellationToken ct)
    {
        var timeout = TimeSpan.FromSeconds(Math.Max(1, Math.Min(req.TimeoutSeconds, 60)));
        var devices = await webOSDiscovery.DiscoverDevicesAsync(timeout, ct);
        await SendOkAsync(devices, ct);
    }
}