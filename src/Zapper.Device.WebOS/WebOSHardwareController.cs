using Zapper.Core.Models;
using Microsoft.Extensions.Logging;

namespace Zapper.Device.WebOS;

public class WebOsHardwareController(IWebOsClient webOsClient, ILogger<WebOsHardwareController> logger) : IWebOsDeviceController
{

    public async Task<bool> SendCommandAsync(Zapper.Core.Models.Device device, DeviceCommand command, CancellationToken cancellationToken = default)
    {
        if (device.ConnectionType != ConnectionType.WebOs)
        {
            logger.LogWarning("Device {DeviceName} is not a WebOS device", device.Name);
            return false;
        }

        if (string.IsNullOrEmpty(device.NetworkAddress))
        {
            logger.LogWarning("Device {DeviceName} has no network address configured", device.Name);
            return false;
        }

        try
        {
            // Connect if not already connected
            if (!webOsClient.IsConnected)
            {
                var connected = await webOsClient.ConnectAsync(device.NetworkAddress, device.UseSecureConnection, cancellationToken);
                if (!connected)
                {
                    logger.LogError("Failed to connect to WebOS device {DeviceName}", device.Name);
                    return false;
                }

                // Authenticate with stored key
                var authenticated = await webOsClient.AuthenticateAsync(device.AuthenticationToken, cancellationToken);
                if (!authenticated)
                {
                    logger.LogError("Failed to authenticate with WebOS device {DeviceName}", device.Name);
                    return false;
                }
            }

            // Execute the command based on type
            return command.Type switch
            {
                CommandType.Power => await webOsClient.PowerOffAsync(cancellationToken),
                CommandType.VolumeUp => await webOsClient.VolumeUpAsync(cancellationToken),
                CommandType.VolumeDown => await webOsClient.VolumeDownAsync(cancellationToken),
                CommandType.Mute => await HandleMute(command, cancellationToken),
                CommandType.ChannelUp => await webOsClient.ChannelUpAsync(cancellationToken),
                CommandType.ChannelDown => await webOsClient.ChannelDownAsync(cancellationToken),
                CommandType.AppLaunch => await HandleLaunchApp(command, cancellationToken),
                CommandType.Input => await HandleSwitchInput(command, cancellationToken),
                CommandType.Custom => await HandleCustomCommand(command, cancellationToken),
                _ => await HandleUnknownCommand(command, cancellationToken)
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send WebOS command {CommandType} to device {DeviceName}", 
                command.Type, device.Name);
            return false;
        }
    }

    private async Task<bool> HandleMute(DeviceCommand command, CancellationToken cancellationToken)
    {
        if (bool.TryParse(command.NetworkPayload, out var muted))
        {
            return await webOsClient.SetMuteAsync(muted, cancellationToken);
        }
        logger.LogWarning("Invalid mute parameter: {Parameters}", command.NetworkPayload);
        return false;
    }

    private async Task<bool> HandleLaunchApp(DeviceCommand command, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(command.NetworkPayload))
        {
            return await webOsClient.LaunchAppAsync(command.NetworkPayload, cancellationToken);
        }
        logger.LogWarning("App ID parameter is required for launch_app command");
        return false;
    }

    private async Task<bool> HandleSwitchInput(DeviceCommand command, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(command.NetworkPayload))
        {
            return await webOsClient.SwitchInputAsync(command.NetworkPayload, cancellationToken);
        }
        logger.LogWarning("Input ID parameter is required for switch_input command");
        return false;
    }

    private async Task<bool> HandleCustomCommand(DeviceCommand command, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(command.HttpEndpoint))
        {
            // For custom commands, use the HttpEndpoint field as the SSAP URI
            var response = await webOsClient.SendCommandAsync(command.HttpEndpoint, 
                string.IsNullOrEmpty(command.NetworkPayload) ? null : command.NetworkPayload, cancellationToken);
            return response != null;
        }
        logger.LogWarning("Custom command requires HttpEndpoint field to be set");
        return false;
    }

    private async Task<bool> HandleUnknownCommand(DeviceCommand command, CancellationToken cancellationToken)
    {
        logger.LogWarning("Unknown WebOS command type: {CommandType}", command.Type);
        
        // Try to execute as a direct SSAP URI if HttpEndpoint field is set
        if (!string.IsNullOrEmpty(command.HttpEndpoint))
        {
            var response = await webOsClient.SendCommandAsync(command.HttpEndpoint, 
                string.IsNullOrEmpty(command.NetworkPayload) ? null : command.NetworkPayload, cancellationToken);
            return response != null;
        }
        
        return false;
    }

    public Task<bool> TestConnectionAsync(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default)
    {
        // For WebOS devices, testing connection means trying to connect and authenticate
        return SendCommandAsync(device, new DeviceCommand 
        { 
            Type = CommandType.Custom, 
            NetworkPayload = "Connection test from ZapperHub",
            HttpEndpoint = "ssap://system.notifications/createToast"
        }, cancellationToken);
    }
}