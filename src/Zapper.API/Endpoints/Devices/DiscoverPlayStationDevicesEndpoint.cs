using FastEndpoints;
using Zapper.Contracts.Devices;
using Zapper.Device.PlayStation;

namespace Zapper.API.Endpoints.Devices;

public class DiscoverPlayStationDevicesEndpoint(IPlayStationDiscovery playStationDiscovery) : EndpointWithoutRequest<IEnumerable<PlayStationDeviceDto>>
{

    public override void Configure()
    {
        Get("/api/devices/discover/playstation");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Discover PlayStation devices on the network";
            s.Description = "Scan the network for PlayStation 4/5 consoles that support Remote Play";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var timeout = TimeSpan.FromSeconds(10); // Default timeout
        var devices = await playStationDiscovery.DiscoverDevices(timeout, ct);

        var response = devices.Select(d => new PlayStationDeviceDto
        {
            Name = d.Name,
            IpAddress = d.IpAddress ?? string.Empty,
            Model = d.Model ?? "PlayStation"
        });

        await SendOkAsync(response, ct);
    }
}