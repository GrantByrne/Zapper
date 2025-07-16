using FastEndpoints;
using Zapper.Contracts.Devices;
using Zapper.Device.WebOS;

namespace Zapper.API.Endpoints.Devices.WebOs;

public class DiscoverWebOsDevicesEndpoint(IWebOsDiscovery webOsDiscovery) : Endpoint<DiscoverWebOsDevicesRequest, IEnumerable<Zapper.Core.Models.Device>>
{

    public override void Configure()
    {
        Post("/api/devices/discover/webos");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Discover WebOS devices on the network";
            s.Description = "Scan the network for LG WebOS smart TVs using SSDP and mDNS discovery. This will find all WebOS devices on the local network.";
            s.ExampleRequest = new DiscoverWebOsDevicesRequest { TimeoutSeconds = 10 };
            s.Responses[200] = "List of discovered WebOS devices";
            s.Responses[500] = "Internal server error during discovery";
        });
        Tags("Devices", "Discovery");
    }

    public override async Task HandleAsync(DiscoverWebOsDevicesRequest req, CancellationToken ct)
    {
        var timeout = TimeSpan.FromSeconds(Math.Max(1, Math.Min(req.TimeoutSeconds, 60)));
        var devices = await webOsDiscovery.DiscoverDevices(timeout, ct);
        await SendOkAsync(devices, ct);
    }
}