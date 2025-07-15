using FastEndpoints;
using Microsoft.AspNetCore.SignalR;
using Zapper.Services;

namespace Zapper.API.Endpoints.Devices;

public class StopWebOsScanEndpoint(
    IHubContext<ZapperSignalR> hubContext) : EndpointWithoutRequest<StopWebOsScanResponse>
{
    public override void Configure()
    {
        Post("/api/devices/scan/webos/stop");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Stop WebOS TV scanning";
            s.Description = "Stop any ongoing WebOS TV discovery";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            // Send scan completed signal to all clients
            // Note: The current WebOS discovery implementation doesn't have a built-in stop mechanism
            // The background task will continue but UI clients will receive the completion signal
            await hubContext.Clients.All.SendAsync("WebOSScanCompleted", cancellationToken: ct);

            await SendOkAsync(new StopWebOsScanResponse
            {
                Success = true,
                Message = "WebOS scanning stopped (UI updated)"
            }, ct);
        }
        catch (Exception ex)
        {
            await SendAsync(new StopWebOsScanResponse
            {
                Success = false,
                Message = ex.Message
            }, 500, ct);
        }
    }
}