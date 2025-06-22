using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Zapper.Services;

public class ZapperSignalR : Hub
{
    private readonly ILogger<ZapperSignalR> _logger;

    public ZapperSignalR(ILogger<ZapperSignalR> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await Groups.AddToGroupAsync(Context.ConnectionId, "AllClients");
        await Clients.Caller.SendAsync("Connected", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "AllClients");
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinDeviceGroup(string deviceId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Device_{deviceId}");
        _logger.LogInformation("Client {ConnectionId} joined device group {DeviceId}", Context.ConnectionId, deviceId);
    }

    public async Task LeaveDeviceGroup(string deviceId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Device_{deviceId}");
        _logger.LogInformation("Client {ConnectionId} left device group {DeviceId}", Context.ConnectionId, deviceId);
    }

    public async Task JoinActivityGroup(string activityId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Activity_{activityId}");
        _logger.LogInformation("Client {ConnectionId} joined activity group {ActivityId}", Context.ConnectionId, activityId);
    }

    public async Task LeaveActivityGroup(string activityId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Activity_{activityId}");
        _logger.LogInformation("Client {ConnectionId} left activity group {ActivityId}", Context.ConnectionId, activityId);
    }

    public async Task SendHeartbeat()
    {
        await Clients.Caller.SendAsync("Heartbeat", DateTime.UtcNow.ToString("O"));
    }
}