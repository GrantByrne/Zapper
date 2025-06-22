using FastEndpoints;
using Zapper.Hardware;
using Zapper.Core.Models;

namespace Zapper.Endpoints.Devices;

public class DiscoverWebOSDevicesRequest
{
    public int TimeoutSeconds { get; set; } = 10;
}

public class DiscoverWebOSDevicesEndpoint : Endpoint<DiscoverWebOSDevicesRequest, IEnumerable<Device>>
{
    public IWebOSDiscovery WebOSDiscovery { get; set; } = null!;

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
        var devices = await WebOSDiscovery.DiscoverDevicesAsync(timeout, ct);
        await SendOkAsync(devices, ct);
    }
}