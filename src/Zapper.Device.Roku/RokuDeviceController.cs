using Microsoft.Extensions.Logging;
using Zapper.Core.Models;
using Zapper.Device.Network;

namespace Zapper.Device.Roku;

public class RokuDeviceController(INetworkDeviceController networkController, ILogger<RokuDeviceController> logger) : IRokuDeviceController
{
    private const int RokuPort = 8060;

    public async Task<bool> SendCommand(Zapper.Core.Models.Device device, DeviceCommand command, CancellationToken cancellationToken = default)
    {
        if (device.ConnectionType != ConnectionType.NetworkHttp)
        {
            logger.LogWarning("Device {DeviceName} is not configured for HTTP connection", device.Name);
            return false;
        }

        if (string.IsNullOrEmpty(device.IpAddress))
        {
            logger.LogWarning("Device {DeviceName} has no IP address configured", device.Name);
            return false;
        }

        try
        {
            return command.Type switch
            {
                CommandType.Power => await SendKey(device.IpAddress, "Power", cancellationToken),
                CommandType.VolumeUp => await SendKey(device.IpAddress, "VolumeUp", cancellationToken),
                CommandType.VolumeDown => await SendKey(device.IpAddress, "VolumeDown", cancellationToken),
                CommandType.Mute => await SendKey(device.IpAddress, "VolumeMute", cancellationToken),
                CommandType.ChannelUp => await SendKey(device.IpAddress, "ChannelUp", cancellationToken),
                CommandType.ChannelDown => await SendKey(device.IpAddress, "ChannelDown", cancellationToken),
                CommandType.Menu => await SendKey(device.IpAddress, "Home", cancellationToken),
                CommandType.Back => await SendKey(device.IpAddress, "Back", cancellationToken),
                CommandType.DirectionalUp => await SendKey(device.IpAddress, "Up", cancellationToken),
                CommandType.DirectionalDown => await SendKey(device.IpAddress, "Down", cancellationToken),
                CommandType.DirectionalLeft => await SendKey(device.IpAddress, "Left", cancellationToken),
                CommandType.DirectionalRight => await SendKey(device.IpAddress, "Right", cancellationToken),
                CommandType.Ok => await SendKey(device.IpAddress, "Select", cancellationToken),
                CommandType.PlayPause => await SendKey(device.IpAddress, "Play", cancellationToken),
                CommandType.Stop => await SendKey(device.IpAddress, "Stop", cancellationToken),
                CommandType.FastForward => await SendKey(device.IpAddress, "Fwd", cancellationToken),
                CommandType.Rewind => await SendKey(device.IpAddress, "Rev", cancellationToken),
                CommandType.Number => await HandleNumberCommand(device.IpAddress, command, cancellationToken),
                CommandType.Input => await SendKey(device.IpAddress, "InputTuner", cancellationToken),
                CommandType.Custom => await HandleCustomCommand(device.IpAddress, command, cancellationToken),
                _ => await HandleUnknownCommand(command, cancellationToken)
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send command {CommandType} to Roku device {DeviceName}", command.Type, device.Name);
            return false;
        }
    }

    public async Task<bool> TestConnection(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(device.IpAddress))
        {
            return false;
        }

        try
        {
            var deviceInfo = await GetDeviceInfo(device.IpAddress, cancellationToken);
            return !string.IsNullOrEmpty(deviceInfo);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to test connection to Roku device {DeviceName}", device.Name);
            return false;
        }
    }

    public async Task<string?> GetDeviceInfo(string ipAddress, CancellationToken cancellationToken = default)
    {
        try
        {
            var baseUrl = $"http://{ipAddress}:{RokuPort}";
            var success = await networkController.SendHttpCommand(baseUrl, "/", "GET", null, null, cancellationToken);

            if (success)
            {
                logger.LogDebug("Successfully retrieved device info from Roku at {IpAddress}", ipAddress);
                return "Roku Device"; // In a real implementation, we'd parse the XML response
            }

            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get device info from Roku at {IpAddress}", ipAddress);
            return null;
        }
    }

    public async Task<bool> LaunchApp(string ipAddress, string appId, CancellationToken cancellationToken = default)
    {
        try
        {
            var baseUrl = $"http://{ipAddress}:{RokuPort}";
            var endpoint = $"/launch/{appId}";

            return await networkController.SendHttpCommand(baseUrl, endpoint, "POST", null, null, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to launch app {AppId} on Roku at {IpAddress}", appId, ipAddress);
            return false;
        }
    }

    public async Task<bool> SendKey(string ipAddress, string keyCode, CancellationToken cancellationToken = default)
    {
        try
        {
            var baseUrl = $"http://{ipAddress}:{RokuPort}";
            var endpoint = $"/keypress/{keyCode}";

            return await networkController.SendHttpCommand(baseUrl, endpoint, "POST", null, null, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send key {KeyCode} to Roku at {IpAddress}", keyCode, ipAddress);
            return false;
        }
    }

    private async Task<bool> HandleNumberCommand(string ipAddress, DeviceCommand command, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(command.NetworkPayload) && int.TryParse(command.NetworkPayload, out var number))
        {
            var keyCode = number switch
            {
                0 => "Lit_0",
                1 => "Lit_1",
                2 => "Lit_2",
                3 => "Lit_3",
                4 => "Lit_4",
                5 => "Lit_5",
                6 => "Lit_6",
                7 => "Lit_7",
                8 => "Lit_8",
                9 => "Lit_9",
                _ => null
            };

            if (!string.IsNullOrEmpty(keyCode))
            {
                return await SendKey(ipAddress, keyCode, cancellationToken);
            }
        }

        logger.LogWarning("Invalid number for number command: {Payload}", command.NetworkPayload);
        return false;
    }

    private async Task<bool> HandleCustomCommand(string ipAddress, DeviceCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(command.NetworkPayload))
        {
            logger.LogWarning("Custom command has no payload");
            return false;
        }

        try
        {
            // Handle app launch commands
            if (command.NetworkPayload.StartsWith("launch:"))
            {
                var appId = command.NetworkPayload[7..];
                return await LaunchApp(ipAddress, appId, cancellationToken);
            }

            // Handle specific app shortcuts
            var shortcutAppId = command.NetworkPayload.ToLowerInvariant() switch
            {
                "netflix" => "12",
                "hulu" => "2285",
                "disney+" or "disney plus" => "291097",
                "amazon prime" or "prime video" => "13",
                "youtube" => "837",
                "roku channel" => "151908",
                "sling tv" => "46041",
                "hbo max" => "61322",
                "spotify" => "22297",
                "apple tv" or "apple tv+" => "551012",
                "paramount+" => "593099",
                "peacock" => "593099",
                _ => null
            };

            if (!string.IsNullOrEmpty(shortcutAppId))
            {
                return await LaunchApp(ipAddress, shortcutAppId, cancellationToken);
            }

            // Try as direct key code
            return await SendKey(ipAddress, command.NetworkPayload, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to handle custom command: {Payload}", command.NetworkPayload);
            return false;
        }
    }

    private Task<bool> HandleUnknownCommand(DeviceCommand command, CancellationToken cancellationToken)
    {
        logger.LogWarning("Unknown Roku command type: {CommandType}", command.Type);
        return Task.FromResult(false);
    }
}