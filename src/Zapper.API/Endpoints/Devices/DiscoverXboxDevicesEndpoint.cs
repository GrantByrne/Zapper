using FastEndpoints;
using Microsoft.AspNetCore.SignalR;
using Zapper.Device.Xbox;
using Zapper.Services;

namespace Zapper.API.Endpoints.Devices;

public class DiscoverXboxDevicesEndpoint(IXboxDiscovery xboxDiscovery, IHubContext<ZapperSignalR> hubContext) : Endpoint<DiscoverXboxDevicesRequest, DiscoverXboxDevicesResponse>
{
    public override void Configure()
    {
        Post("/api/devices/discover/xbox");
        AllowAnonymous();
    }

    public override async Task HandleAsync(DiscoverXboxDevicesRequest req, CancellationToken ct)
    {
        var connectionId = HttpContext.Request.Headers["X-SignalR-ConnectionId"].FirstOrDefault();

        if (!string.IsNullOrEmpty(connectionId))
        {
            xboxDiscovery.DeviceFound += async (sender, device) =>
            {
                await hubContext.Clients.Client(connectionId).SendAsync("XboxDeviceFound", new
                {
                    name = device.Name,
                    ipAddress = device.IpAddress,
                    liveId = device.LiveId,
                    consoleType = device.ConsoleType.ToString(),
                    isAuthenticated = device.IsAuthenticated
                }, ct);
            };
        }

        var devices = await xboxDiscovery.DiscoverDevices(TimeSpan.FromSeconds(req.DurationSeconds), ct);

        await SendOkAsync(new DiscoverXboxDevicesResponse
        {
            Success = true,
            Devices = devices.Select(d => new XboxDeviceDto
            {
                Name = d.Name,
                IpAddress = d.IpAddress,
                LiveId = d.LiveId,
                ConsoleType = d.ConsoleType.ToString(),
                IsAuthenticated = d.IsAuthenticated
            }).ToList()
        }, ct);
    }
}