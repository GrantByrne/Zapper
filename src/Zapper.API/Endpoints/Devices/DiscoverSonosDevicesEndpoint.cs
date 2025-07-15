using FastEndpoints;
using Zapper.API.Models.Requests;
using Zapper.Device.Sonos;

namespace Zapper.API.Endpoints.Devices;

public class DiscoverSonosDevicesEndpoint(ISonosDiscovery sonosDiscovery) : Endpoint<DiscoverSonosDevicesRequest, IEnumerable<Zapper.Core.Models.Device>>
{

    public override void Configure()
    {
        Post("/api/devices/discover/sonos");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Discover Sonos devices on the network";
            s.Description = "Scan the network for Sonos speakers using SSDP discovery";
        });
    }

    public override async Task HandleAsync(DiscoverSonosDevicesRequest req, CancellationToken ct)
    {
        var timeout = TimeSpan.FromSeconds(Math.Max(1, Math.Min(req.TimeoutSeconds, 60)));
        var devices = await sonosDiscovery.DiscoverDevicesAsync(timeout, ct);
        await SendOkAsync(devices, ct);
    }
}