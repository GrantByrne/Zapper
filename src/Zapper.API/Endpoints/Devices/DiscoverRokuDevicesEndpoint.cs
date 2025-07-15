using FastEndpoints;
using Zapper.Contracts.Devices;
using Zapper.Device.Roku;

namespace Zapper.API.Endpoints.Devices;

public class DiscoverRokuDevicesEndpoint(IRokuDiscovery rokuDiscovery) : Endpoint<DiscoverRokuDevicesRequest, IEnumerable<RokuDeviceDto>>
{

    public override void Configure()
    {
        Post("/api/devices/discover/roku");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Discover Roku devices on the network";
            s.Description = "Scan the network for Roku streaming devices using SSDP discovery";
        });
    }

    public override async Task HandleAsync(DiscoverRokuDevicesRequest req, CancellationToken ct)
    {
        var timeout = TimeSpan.FromSeconds(Math.Max(1, Math.Min(req.TimeoutSeconds, 60)));
        var devices = await rokuDiscovery.DiscoverDevices(timeout, ct);

        var rokuDevices = devices.Select(d => new RokuDeviceDto
        {
            Name = d.Name,
            IpAddress = d.IpAddress ?? "",
            Model = d.Model,
            SerialNumber = null,
            Port = d.Port ?? 8060
        });

        await SendOkAsync(rokuDevices, ct);
    }
}