using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Zapper.Device.USB;

namespace Zapper.Services;

public class UsbRemoteEventHandler : IHostedService
{
    private readonly IUsbRemoteHandler _remoteHandler;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<UsbRemoteEventHandler> _logger;

    public UsbRemoteEventHandler(
        IUsbRemoteHandler remoteHandler,
        IServiceScopeFactory scopeFactory,
        ILogger<UsbRemoteEventHandler> logger)
    {
        _remoteHandler = remoteHandler;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Wire up event handlers
        _remoteHandler.ButtonPressed += OnButtonPressed;
        _remoteHandler.RemoteConnected += OnRemoteConnected;
        _remoteHandler.RemoteDisconnected += OnRemoteDisconnected;

        _logger.LogInformation("USB remote event handler started");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        // Unwire event handlers
        _remoteHandler.ButtonPressed -= OnButtonPressed;
        _remoteHandler.RemoteConnected -= OnRemoteConnected;
        _remoteHandler.RemoteDisconnected -= OnRemoteDisconnected;

        _logger.LogInformation("USB remote event handler stopped");
        return Task.CompletedTask;
    }

    private async void OnButtonPressed(object? sender, RemoteButtonEventArgs e)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IUsbRemoteService>();
            await service.ExecuteButtonMappingAsync(e.DeviceId, (byte)e.KeyCode, e.EventType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling button press for device {DeviceId}", e.DeviceId);
        }
    }

    private async void OnRemoteConnected(object? sender, string deviceId)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IUsbRemoteService>();
            await service.HandleRemoteConnectedAsync(deviceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling remote connection for device {DeviceId}", deviceId);
        }
    }

    private async void OnRemoteDisconnected(object? sender, string deviceId)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IUsbRemoteService>();
            await service.HandleRemoteDisconnectedAsync(deviceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling remote disconnection for device {DeviceId}", deviceId);
        }
    }
}