using FastEndpoints;
using Zapper.Device.AppleTV.Models;
using Zapper.Device.AppleTV.Services;

namespace Zapper.API.Endpoints.Devices.AppleTV;

public class DiscoverAppleTvRequest
{
    public int TimeoutSeconds { get; set; } = 5;
}

public class DiscoverAppleTvResponse
{
    public List<AppleTvDevice> Devices { get; set; } = new();
}

public class DiscoverAppleTvEndpoint(AppleTvService appleTvService)
    : Endpoint<DiscoverAppleTvRequest, DiscoverAppleTvResponse>
{
    public override void Configure()
    {
        Post("/api/devices/appletv/discover");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Discover Apple TV devices on the network";
            s.Description = "Uses mDNS/Bonjour to discover Apple TV devices";
        });
    }

    public override async Task HandleAsync(DiscoverAppleTvRequest req, CancellationToken ct)
    {
        var devices = await appleTvService.DiscoverAppleTvsAsync(req.TimeoutSeconds);

        await SendOkAsync(new DiscoverAppleTvResponse
        {
            Devices = devices
        }, ct);
    }
}