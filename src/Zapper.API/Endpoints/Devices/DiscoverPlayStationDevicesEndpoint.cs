using FastEndpoints;
using Zapper.API.Models.Requests;
using Zapper.Device.PlayStation;

namespace Zapper.API.Endpoints.Devices;

public class DiscoverPlayStationDevicesEndpoint(IPlayStationDiscovery playStationDiscovery) : Endpoint<DiscoverPlayStationDevicesRequest, IEnumerable<Zapper.Core.Models.Device>>
{

    public override void Configure()
    {
        Post("/api/devices/discover/playstation");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Discover PlayStation devices on the network";
            s.Description = "Scan the network for PlayStation 4/5 consoles that support Remote Play";
        });
    }

    public override async Task HandleAsync(DiscoverPlayStationDevicesRequest req, CancellationToken ct)
    {
        var timeout = TimeSpan.FromSeconds(Math.Max(1, Math.Min(req.TimeoutSeconds, 60)));
        var devices = await playStationDiscovery.DiscoverDevicesAsync(timeout, ct);
        await SendOkAsync(devices, ct);
    }
}