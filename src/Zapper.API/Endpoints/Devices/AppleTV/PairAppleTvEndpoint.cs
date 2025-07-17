using FastEndpoints;
using Zapper.Device.AppleTV.Services;

namespace Zapper.API.Endpoints.Devices.AppleTV;

public class PairAppleTvRequest
{
    public int DeviceId { get; set; }
    public required string Pin { get; set; }
}

public class PairAppleTvResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
}

public class PairAppleTvEndpoint(AppleTvService appleTvService)
    : Endpoint<PairAppleTvRequest, PairAppleTvResponse>
{
    public override void Configure()
    {
        Post("/api/devices/appletv/{DeviceId}/pair");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Pair with an Apple TV device";
            s.Description = "Initiates pairing with an Apple TV using the provided PIN";
        });
    }

    public override async Task HandleAsync(PairAppleTvRequest req, CancellationToken ct)
    {
        var success = await appleTvService.PairDeviceAsync(req.DeviceId, req.Pin);

        await SendOkAsync(new PairAppleTvResponse
        {
            Success = success,
            Message = success ? "Successfully paired with Apple TV" : "Failed to pair with Apple TV"
        }, ct);
    }
}