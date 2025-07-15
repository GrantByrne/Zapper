using FastEndpoints;
using Microsoft.AspNetCore.SignalR;
using Zapper.Device.Xbox;
using Zapper.Device.Xbox.Models;
using Zapper.Services;

namespace Zapper.Endpoints.Devices;

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

        var devices = await xboxDiscovery.DiscoverDevicesAsync(TimeSpan.FromSeconds(req.DurationSeconds), ct);

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

public class DiscoverXboxDevicesRequest
{
    public int DurationSeconds { get; set; } = 15;
}

public class DiscoverXboxDevicesResponse
{
    public bool Success { get; set; }
    public List<XboxDeviceDto> Devices { get; set; } = new();
}

public class XboxDeviceDto
{
    public string Name { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string LiveId { get; set; } = string.Empty;
    public string ConsoleType { get; set; } = string.Empty;
    public bool IsAuthenticated { get; set; }
}