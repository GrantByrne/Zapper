using FastEndpoints;
using Zapper.API.Models.Requests;
using Zapper.Device.Yamaha;

namespace Zapper.API.Endpoints.Devices;

public class DiscoverYamahaDevicesEndpoint(IYamahaDiscovery yamahaDiscovery) : Endpoint<DiscoverYamahaDevicesRequest, IEnumerable<Zapper.Core.Models.Device>>
{

    public override void Configure()
    {
        Post("/api/devices/discover/yamaha");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Discover Yamaha devices on the network";
            s.Description = "Scan the network for Yamaha receivers with MusicCast support";
        });
    }

    public override async Task HandleAsync(DiscoverYamahaDevicesRequest req, CancellationToken ct)
    {
        var timeout = TimeSpan.FromSeconds(Math.Max(1, Math.Min(req.TimeoutSeconds, 60)));
        var devices = await yamahaDiscovery.DiscoverDevicesAsync(timeout, ct);
        await SendOkAsync(devices, ct);
    }
}