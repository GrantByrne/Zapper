using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Zapper.Device.USB;

public class UsbRemoteHostedService(
    IUsbRemoteHandler remoteHandler,
    ILogger<UsbRemoteHostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting USB remote service");

        try
        {
            await remoteHandler.StartListening(cancellationToken);

            // Subscribe to button events for logging
            remoteHandler.ButtonPressed += OnButtonPressed;
            remoteHandler.ButtonDown += OnButtonDown;
            remoteHandler.ButtonUp += OnButtonUp;
            remoteHandler.ButtonLongPress += OnButtonLongPress;
            remoteHandler.RemoteConnected += OnRemoteConnected;
            remoteHandler.RemoteDisconnected += OnRemoteDisconnected;

            logger.LogInformation("USB remote service started successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to start USB remote service");
            throw;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Stopping USB remote service");

        try
        {
            remoteHandler.ButtonPressed -= OnButtonPressed;
            remoteHandler.ButtonDown -= OnButtonDown;
            remoteHandler.ButtonUp -= OnButtonUp;
            remoteHandler.ButtonLongPress -= OnButtonLongPress;
            remoteHandler.RemoteConnected -= OnRemoteConnected;
            remoteHandler.RemoteDisconnected -= OnRemoteDisconnected;
            await remoteHandler.StopListening();

            logger.LogInformation("USB remote service stopped");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error stopping USB remote service");
        }
    }

    private void OnButtonPressed(object? sender, RemoteButtonEventArgs e)
    {
        logger.LogInformation("USB remote button pressed: Device={DeviceId}, Button={ButtonName}, KeyCode=0x{KeyCode:X2}",
            e.DeviceId, e.ButtonName, e.KeyCode);
    }

    private void OnButtonDown(object? sender, RemoteButtonEventArgs e)
    {
        logger.LogDebug("USB remote button down: Device={DeviceId}, Button={ButtonName}",
            e.DeviceId, e.ButtonName);
    }

    private void OnButtonUp(object? sender, RemoteButtonEventArgs e)
    {
        logger.LogDebug("USB remote button up: Device={DeviceId}, Button={ButtonName}, Duration={Duration}ms",
            e.DeviceId, e.ButtonName, e.HoldDuration?.TotalMilliseconds ?? 0);
    }

    private void OnButtonLongPress(object? sender, RemoteButtonEventArgs e)
    {
        logger.LogInformation("USB remote long press: Device={DeviceId}, Button={ButtonName}, Duration={Duration}ms",
            e.DeviceId, e.ButtonName, e.HoldDuration?.TotalMilliseconds ?? 0);
    }

    private void OnRemoteConnected(object? sender, string deviceId)
    {
        logger.LogInformation("USB remote connected: {DeviceId}", deviceId);
    }

    private void OnRemoteDisconnected(object? sender, string deviceId)
    {
        logger.LogInformation("USB remote disconnected: {DeviceId}", deviceId);
    }
}