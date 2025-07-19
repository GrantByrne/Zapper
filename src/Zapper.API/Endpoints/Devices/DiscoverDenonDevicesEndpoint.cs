using FastEndpoints;
using Zapper.Client.Devices;
using Zapper.Device.Denon;

namespace Zapper.API.Endpoints.Devices;

public class DiscoverDenonDevicesEndpoint(IDenonDiscovery denonDiscovery) : Endpoint<DiscoverDenonDevicesRequest, IEnumerable<DenonDeviceDto>>
{
    public override void Configure()
    {
        Post("/api/devices/discover/denon");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Discover Denon/Marantz devices on the network";
            s.Description = "Scan the network for Denon and Marantz AV receivers";
        });
    }

    public override async Task HandleAsync(DiscoverDenonDevicesRequest req, CancellationToken ct)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, Math.Min(req.TimeoutSeconds, 60))));

        var devices = await denonDiscovery.DiscoverDevicesAsync(cts.Token);

        var dtos = devices.Select(d => new DenonDeviceDto
        {
            Name = d.Name,
            IpAddress = d.IpAddress,
            Model = d.Model,
            SerialNumber = d.SerialNumber
        });

        await SendOkAsync(dtos, ct);
    }
}