using FastEndpoints;
using Microsoft.AspNetCore.SignalR;
using Zapper.Device.WebOS;
using Zapper.Services;

namespace Zapper.API.Endpoints.Devices;

public class WebOsScanEndpoint(
    IWebOsDiscovery webOsDiscovery,
    IHubContext<ZapperSignalR> hubContext) : Endpoint<WebOsScanRequest, WebOsScanResponse>
{
    public override void Configure()
    {
        Post("/api/devices/scan/webos");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Start WebOS TV discovery scanning";
            s.Description = "Start scanning for discoverable WebOS TVs on the network and send real-time updates via SignalR";
        });
    }

    public override async Task HandleAsync(WebOsScanRequest req, CancellationToken ct)
    {
        try
        {
            // Notify clients that scanning started
            await hubContext.Clients.All.SendAsync("WebOSScanStarted", cancellationToken: ct);

            // Start discovery process
            _ = Task.Run(async () =>
            {
                try
                {
                    var devices = await webOsDiscovery.DiscoverDevices(TimeSpan.FromSeconds(req.DurationSeconds), ct);

                    foreach (var device in devices)
                    {
                        // Send real-time device discovery updates via SignalR
                        await hubContext.Clients.All.SendAsync("WebOSDeviceFound", new
                        {
                            Name = device.Name,
                            IpAddress = device.IpAddress,
                            Port = device.Port,
                            ModelName = device.Model,
                            ModelNumber = device.Brand
                        }, cancellationToken: ct);
                    }

                    await Task.Delay(TimeSpan.FromSeconds(req.DurationSeconds), ct);
                    await hubContext.Clients.All.SendAsync("WebOSScanCompleted", cancellationToken: ct);
                }
                catch (Exception ex)
                {
                    await hubContext.Clients.All.SendAsync("WebOSScanError", ex.Message, cancellationToken: ct);
                }
            }, ct);

            await SendOkAsync(new WebOsScanResponse
            {
                Success = true,
                Message = $"WebOS TV scanning started for {req.DurationSeconds} seconds",
                IsScanning = true
            }, ct);
        }
        catch (Exception ex)
        {
            await SendAsync(new WebOsScanResponse
            {
                Success = false,
                Message = ex.Message,
                IsScanning = false
            }, 500, ct);
        }
    }
}