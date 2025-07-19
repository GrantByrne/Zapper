using FastEndpoints;
using Zapper.Device.AppleTV.Services;

namespace Zapper.API.Endpoints.Devices.AppleTV;

public class GetAppleTvStatusEndpoint(AppleTvService appleTvService)
    : Endpoint<GetAppleTvStatusRequest, GetAppleTvStatusResponse>
{
    public override void Configure()
    {
        Get("/api/devices/appletv/{DeviceId}/status");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get Apple TV playback status";
            s.Description = "Retrieves the current playback status from an Apple TV";
        });
    }

    public override async Task HandleAsync(GetAppleTvStatusRequest req, CancellationToken ct)
    {
        var status = await appleTvService.GetStatusAsync(req.DeviceId);

        if (status != null)
        {
            await SendOkAsync(new GetAppleTvStatusResponse
            {
                Status = status,
                Success = true
            }, ct);
        }
        else
        {
            await SendAsync(new GetAppleTvStatusResponse
            {
                Success = false,
                Message = "Failed to get Apple TV status - device may not be connected"
            }, 400, ct);
        }
    }
}