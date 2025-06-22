using Zapper.Core.Models;
using Microsoft.Extensions.Logging;

namespace Zapper.Hardware;

public class WebOSDeviceController : IWebOSDeviceController
{
    private readonly IWebOSClient _webOSClient;
    private readonly ILogger<WebOSDeviceController> _logger;

    public WebOSDeviceController(IWebOSClient webOSClient, ILogger<WebOSDeviceController> logger)
    {
        _webOSClient = webOSClient;
        _logger = logger;
    }

    public async Task<bool> SendCommandAsync(Device device, DeviceCommand command, CancellationToken cancellationToken = default)
    {
        if (device.ConnectionType != ConnectionType.WebOS)
        {
            _logger.LogWarning("Device {DeviceName} is not a WebOS device", device.Name);
            return false;
        }

        if (string.IsNullOrEmpty(device.NetworkAddress))
        {
            _logger.LogWarning("Device {DeviceName} has no network address configured", device.Name);
            return false;
        }

        try
        {
            // Connect if not already connected
            if (!_webOSClient.IsConnected)
            {
                var connected = await _webOSClient.ConnectAsync(device.NetworkAddress, device.UseSecureConnection, cancellationToken);
                if (!connected)
                {
                    _logger.LogError("Failed to connect to WebOS device {DeviceName}", device.Name);
                    return false;
                }

                // Authenticate with stored key
                var authenticated = await _webOSClient.AuthenticateAsync(device.AuthenticationToken, cancellationToken);
                if (!authenticated)
                {
                    _logger.LogError("Failed to authenticate with WebOS device {DeviceName}", device.Name);
                    return false;
                }
            }

            // Execute the command based on type
            return command.Type switch
            {
                CommandType.Power => await _webOSClient.PowerOffAsync(cancellationToken),
                CommandType.VolumeUp => await _webOSClient.VolumeUpAsync(cancellationToken),
                CommandType.VolumeDown => await _webOSClient.VolumeDownAsync(cancellationToken),
                CommandType.Mute => await HandleMute(command, cancellationToken),
                CommandType.ChannelUp => await _webOSClient.ChannelUpAsync(cancellationToken),
                CommandType.ChannelDown => await _webOSClient.ChannelDownAsync(cancellationToken),
                CommandType.AppLaunch => await HandleLaunchApp(command, cancellationToken),
                CommandType.Input => await HandleSwitchInput(command, cancellationToken),
                CommandType.Custom => await HandleCustomCommand(command, cancellationToken),
                _ => await HandleUnknownCommand(command, cancellationToken)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send WebOS command {CommandType} to device {DeviceName}", 
                command.Type, device.Name);
            return false;
        }
    }

    private async Task<bool> HandleMute(DeviceCommand command, CancellationToken cancellationToken)
    {
        if (bool.TryParse(command.NetworkPayload, out var muted))
        {
            return await _webOSClient.SetMuteAsync(muted, cancellationToken);
        }
        _logger.LogWarning("Invalid mute parameter: {Parameters}", command.NetworkPayload);
        return false;
    }

    private async Task<bool> HandleLaunchApp(DeviceCommand command, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(command.NetworkPayload))
        {
            return await _webOSClient.LaunchAppAsync(command.NetworkPayload, cancellationToken);
        }
        _logger.LogWarning("App ID parameter is required for launch_app command");
        return false;
    }

    private async Task<bool> HandleSwitchInput(DeviceCommand command, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(command.NetworkPayload))
        {
            return await _webOSClient.SwitchInputAsync(command.NetworkPayload, cancellationToken);
        }
        _logger.LogWarning("Input ID parameter is required for switch_input command");
        return false;
    }

    private async Task<bool> HandleCustomCommand(DeviceCommand command, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(command.HttpEndpoint))
        {
            // For custom commands, use the HttpEndpoint field as the SSAP URI
            var response = await _webOSClient.SendCommandAsync(command.HttpEndpoint, 
                string.IsNullOrEmpty(command.NetworkPayload) ? null : command.NetworkPayload, cancellationToken);
            return response != null;
        }
        _logger.LogWarning("Custom command requires HttpEndpoint field to be set");
        return false;
    }

    private async Task<bool> HandleUnknownCommand(DeviceCommand command, CancellationToken cancellationToken)
    {
        _logger.LogWarning("Unknown WebOS command type: {CommandType}", command.Type);
        
        // Try to execute as a direct SSAP URI if HttpEndpoint field is set
        if (!string.IsNullOrEmpty(command.HttpEndpoint))
        {
            var response = await _webOSClient.SendCommandAsync(command.HttpEndpoint, 
                string.IsNullOrEmpty(command.NetworkPayload) ? null : command.NetworkPayload, cancellationToken);
            return response != null;
        }
        
        return false;
    }

    public Task<bool> TestConnectionAsync(Device device, CancellationToken cancellationToken = default)
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