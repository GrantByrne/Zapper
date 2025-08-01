using FastEndpoints;
using Zapper.Client.Devices;
using Zapper.Device.Sonos;

namespace Zapper.API.Endpoints.Devices;

public class DiscoverSonosDevicesEndpoint(ISonosDiscovery sonosDiscovery) : Endpoint<DiscoverSonosDevicesRequest, IEnumerable<SonosDeviceDto>>
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
        var devices = await sonosDiscovery.DiscoverDevices(timeout, ct);

        var dtos = devices.Select(d => new SonosDeviceDto
        {
            Name = d.Name,
            IpAddress = d.IpAddress ?? "",
            Model = d.Model,
            Zone = null, // Will be populated from Sonos-specific data if available
            RoomName = null, // Will be populated from Sonos-specific data if available
            SerialNumber = null // Will be populated from Sonos-specific data if available
        });

        await SendOkAsync(dtos, ct);
    }
}