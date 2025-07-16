using FastEndpoints;
using Zapper.Contracts.Devices;
using Zapper.Device.Yamaha;

namespace Zapper.API.Endpoints.Devices;

public class DiscoverYamahaDevicesEndpoint(IYamahaDiscovery yamahaDiscovery) : Endpoint<DiscoverYamahaDevicesRequest, IEnumerable<YamahaDeviceDto>>
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
        var devices = await yamahaDiscovery.DiscoverDevices(timeout, ct);

        var dtos = devices.Select(d => new YamahaDeviceDto
        {
            Name = d.Name,
            IpAddress = d.IpAddress ?? "",
            Model = d.Model,
            Zone = null, // Will be populated from Yamaha-specific data if available
            Version = null // Will be populated from Yamaha-specific data if available
        });

        await SendOkAsync(dtos, ct);
    }
}