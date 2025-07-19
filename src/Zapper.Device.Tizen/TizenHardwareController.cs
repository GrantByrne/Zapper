using Zapper.Core.Models;
using Microsoft.Extensions.Logging;

namespace Zapper.Device.Tizen;

public class TizenHardwareController(ITizenClient tizenClient, ILogger<TizenHardwareController> logger) : ITizenDeviceController
{
    public async Task<bool> SendCommand(Zapper.Core.Models.Device device, DeviceCommand command, CancellationToken cancellationToken = default)
    {
        if (device.ConnectionType != ConnectionType.Tizen)
        {
            logger.LogWarning("Device {DeviceName} is not a Tizen device", device.Name);
            return false;
        }

        if (string.IsNullOrEmpty(device.NetworkAddress))
        {
            logger.LogWarning("Device {DeviceName} has no network address configured", device.Name);
            return false;
        }

        try
        {
            if (!tizenClient.IsConnected)
            {
                var connected = await tizenClient.ConnectAsync(device.NetworkAddress, device.AuthenticationToken, cancellationToken);
                if (!connected)
                {
                    logger.LogError("Failed to connect to Tizen device {DeviceName}", device.Name);
                    return false;
                }
            }

            return command.Type switch
            {
                CommandType.Power => await tizenClient.PowerOffAsync(cancellationToken),
                CommandType.VolumeUp => await tizenClient.VolumeUpAsync(cancellationToken),
                CommandType.VolumeDown => await tizenClient.VolumeDownAsync(cancellationToken),
                CommandType.Mute => await HandleMute(command, cancellationToken),
                CommandType.ChannelUp => await tizenClient.ChannelUpAsync(cancellationToken),
                CommandType.ChannelDown => await tizenClient.ChannelDownAsync(cancellationToken),
                CommandType.AppLaunch => await HandleLaunchApp(command, cancellationToken),
                CommandType.Input => await HandleSwitchInput(command, cancellationToken),
                CommandType.DirectionalUp => await tizenClient.SendKeyAsync("KEY_UP", cancellationToken),
                CommandType.DirectionalDown => await tizenClient.SendKeyAsync("KEY_DOWN", cancellationToken),
                CommandType.DirectionalLeft => await tizenClient.SendKeyAsync("KEY_LEFT", cancellationToken),
                CommandType.DirectionalRight => await tizenClient.SendKeyAsync("KEY_RIGHT", cancellationToken),
                CommandType.Ok => await tizenClient.SendKeyAsync("KEY_ENTER", cancellationToken),
                CommandType.Back => await tizenClient.SendKeyAsync("KEY_RETURN", cancellationToken),
                CommandType.Home => await tizenClient.SendKeyAsync("KEY_HOME", cancellationToken),
                CommandType.Menu => await tizenClient.SendKeyAsync("KEY_MENU", cancellationToken),
                CommandType.PlayPause => await tizenClient.SendKeyAsync("KEY_PLAY", cancellationToken),
                CommandType.Stop => await tizenClient.SendKeyAsync("KEY_STOP", cancellationToken),
                CommandType.FastForward => await tizenClient.SendKeyAsync("KEY_FF", cancellationToken),
                CommandType.Rewind => await tizenClient.SendKeyAsync("KEY_REWIND", cancellationToken),
                CommandType.KeyboardInput => await HandleKeyboardInput(command, cancellationToken),
                CommandType.Custom => await HandleCustomCommand(command, cancellationToken),
                _ => await HandleUnknownCommand(command, cancellationToken)
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send Tizen command {CommandType} to device {DeviceName}",
                command.Type, device.Name);
            return false;
        }
    }

    private async Task<bool> HandleMute(DeviceCommand command, CancellationToken cancellationToken)
    {
        if (bool.TryParse(command.NetworkPayload, out var muted))
        {
            return await tizenClient.SetMuteAsync(muted, cancellationToken);
        }
        return await tizenClient.SendKeyAsync("KEY_MUTE", cancellationToken);
    }

    private async Task<bool> HandleLaunchApp(DeviceCommand command, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(command.NetworkPayload))
        {
            return await tizenClient.LaunchAppAsync(command.NetworkPayload, cancellationToken);
        }
        logger.LogWarning("App ID parameter is required for launch_app command");
        return false;
    }

    private async Task<bool> HandleSwitchInput(DeviceCommand command, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(command.NetworkPayload))
        {
            return await tizenClient.SwitchInputAsync(command.NetworkPayload, cancellationToken);
        }
        logger.LogWarning("Input ID parameter is required for switch_input command");
        return false;
    }

    private async Task<bool> HandleKeyboardInput(DeviceCommand command, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(command.NetworkPayload))
        {
            return await tizenClient.SendTextAsync(command.NetworkPayload, cancellationToken);
        }
        logger.LogWarning("Keyboard input command requires text parameter");
        return false;
    }

    private async Task<bool> HandleCustomCommand(DeviceCommand command, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(command.IrCode))
        {
            return await tizenClient.SendKeyAsync(command.IrCode, cancellationToken);
        }
        else if (!string.IsNullOrEmpty(command.HttpEndpoint))
        {
            return await tizenClient.SendCommandAsync(command.HttpEndpoint, command.NetworkPayload, cancellationToken);
        }
        logger.LogWarning("Custom command requires IrCode or HttpEndpoint field to be set");
        return false;
    }

    private async Task<bool> HandleUnknownCommand(DeviceCommand command, CancellationToken cancellationToken)
    {
        logger.LogWarning("Unknown Tizen command type: {CommandType}", command.Type);

        if (!string.IsNullOrEmpty(command.IrCode))
        {
            return await tizenClient.SendKeyAsync(command.IrCode, cancellationToken);
        }

        return false;
    }

    public async Task<bool> TestConnection(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default)
    {
        return await SendCommand(device, new DeviceCommand
        {
            Type = CommandType.Custom,
            IrCode = "KEY_INFO"
        }, cancellationToken);
    }
}