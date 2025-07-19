using Zapper.Core.Models;
using Microsoft.Extensions.Logging;

namespace Zapper.Device.Bluetooth;

public class SteamDeckBluetoothController(
    IBluetoothHidController hidController,
    IBluetoothService bluetoothService,
    ILogger<SteamDeckBluetoothController> logger)
    : IBluetoothDeviceController
{
    public async Task<bool> SendCommand(Zapper.Core.Models.Device device, DeviceCommand command, CancellationToken cancellationToken = default)
    {
        if (device.ConnectionType != ConnectionType.Bluetooth)
        {
            logger.LogWarning("Device {DeviceName} is not a Bluetooth device", device.Name);
            return false;
        }

        if (string.IsNullOrEmpty(device.MacAddress))
        {
            logger.LogWarning("Device {DeviceName} has no Bluetooth MAC address configured", device.Name);
            return false;
        }

        try
        {
            var isConnected = await hidController.IsConnected(device.MacAddress, cancellationToken);
            if (!isConnected)
            {
                logger.LogInformation("Connecting to Steam Deck device {DeviceName} ({Address})", device.Name, device.MacAddress);
                var connected = await hidController.Connect(device.MacAddress, cancellationToken);
                if (!connected)
                {
                    logger.LogError("Failed to connect to Bluetooth device {DeviceName}", device.Name);
                    return false;
                }
            }

            return command.Type switch
            {
                CommandType.Power => await HandlePowerCommand(device.MacAddress, command, cancellationToken),
                CommandType.Menu => await hidController.SendKey(device.MacAddress, HidKeyCode.GamepadSteam, cancellationToken),
                CommandType.Back => await hidController.SendKey(device.MacAddress, HidKeyCode.GamepadSelect, cancellationToken),
                CommandType.Home => await hidController.SendKey(device.MacAddress, HidKeyCode.GamepadSteam, cancellationToken),
                CommandType.Ok => await hidController.SendKey(device.MacAddress, HidKeyCode.GamepadA, cancellationToken),
                CommandType.DirectionalUp => await hidController.SendKey(device.MacAddress, HidKeyCode.DPadUp, cancellationToken),
                CommandType.DirectionalDown => await hidController.SendKey(device.MacAddress, HidKeyCode.DPadDown, cancellationToken),
                CommandType.DirectionalLeft => await hidController.SendKey(device.MacAddress, HidKeyCode.DPadLeft, cancellationToken),
                CommandType.DirectionalRight => await hidController.SendKey(device.MacAddress, HidKeyCode.DPadRight, cancellationToken),
                CommandType.PlayPause => await hidController.SendKey(device.MacAddress, HidKeyCode.PlayPause, cancellationToken),
                CommandType.Stop => await hidController.SendKey(device.MacAddress, HidKeyCode.Stop, cancellationToken),
                CommandType.FastForward => await hidController.SendKey(device.MacAddress, HidKeyCode.GamepadRightBumper, cancellationToken),
                CommandType.Rewind => await hidController.SendKey(device.MacAddress, HidKeyCode.GamepadLeftBumper, cancellationToken),
                CommandType.VolumeUp => await hidController.SendKey(device.MacAddress, HidKeyCode.VolumeUp, cancellationToken),
                CommandType.VolumeDown => await hidController.SendKey(device.MacAddress, HidKeyCode.VolumeDown, cancellationToken),
                CommandType.Mute => await hidController.SendKey(device.MacAddress, HidKeyCode.VolumeMute, cancellationToken),
                CommandType.Custom => await HandleCustomCommand(device.MacAddress, command, cancellationToken),
                _ => await HandleUnknownCommand(command, cancellationToken)
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send Bluetooth command {CommandType} to device {DeviceName}",
                command.Type, device.Name);
            return false;
        }
    }

    public async Task<bool> TestConnection(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default)
    {
        if (device.ConnectionType != ConnectionType.Bluetooth)
        {
            return false;
        }

        if (string.IsNullOrEmpty(device.MacAddress))
        {
            return false;
        }

        try
        {
            var deviceInfo = await bluetoothService.GetDevice(device.MacAddress, cancellationToken);
            if (deviceInfo == null)
            {
                return false;
            }

            if (!deviceInfo.IsConnected)
            {
                return await bluetoothService.ConnectDevice(device.MacAddress, cancellationToken);
            }

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to test connection to device {DeviceName}", device.Name);
            return false;
        }
    }

    public async Task<IEnumerable<string>> DiscoverPairedDevices(CancellationToken cancellationToken = default)
    {
        try
        {
            var devices = await bluetoothService.GetDevices(cancellationToken);
            return devices
                .Where(d => d.IsPaired && IsSteamDeckDevice(d))
                .Select(d => d.Address)
                .ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to discover paired Steam Deck devices");
            return [];
        }
    }

    private async Task<bool> HandlePowerCommand(string deviceAddress, DeviceCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Power command requested - using Steam button to wake/show Steam interface");
        return await hidController.SendKey(deviceAddress, HidKeyCode.GamepadSteam, cancellationToken);
    }

    private async Task<bool> HandleCustomCommand(string deviceAddress, DeviceCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(command.NetworkPayload))
        {
            logger.LogWarning("Custom command has no payload");
            return false;
        }

        try
        {
            switch (command.NetworkPayload.ToLowerInvariant())
            {
                case "a":
                case "cross":
                    return await hidController.SendKey(deviceAddress, HidKeyCode.GamepadA, cancellationToken);

                case "b":
                case "circle":
                    return await hidController.SendKey(deviceAddress, HidKeyCode.GamepadB, cancellationToken);

                case "x":
                case "square":
                    return await hidController.SendKey(deviceAddress, HidKeyCode.GamepadX, cancellationToken);

                case "y":
                case "triangle":
                    return await hidController.SendKey(deviceAddress, HidKeyCode.GamepadY, cancellationToken);

                case "lb":
                case "l1":
                case "left_bumper":
                    return await hidController.SendKey(deviceAddress, HidKeyCode.GamepadLeftBumper, cancellationToken);

                case "rb":
                case "r1":
                case "right_bumper":
                    return await hidController.SendKey(deviceAddress, HidKeyCode.GamepadRightBumper, cancellationToken);

                case "lt":
                case "l2":
                case "left_trigger":
                    return await hidController.SendKey(deviceAddress, HidKeyCode.GamepadLeftTrigger, cancellationToken);

                case "rt":
                case "r2":
                case "right_trigger":
                    return await hidController.SendKey(deviceAddress, HidKeyCode.GamepadRightTrigger, cancellationToken);

                case "select":
                case "view":
                    return await hidController.SendKey(deviceAddress, HidKeyCode.GamepadSelect, cancellationToken);

                case "start":
                case "menu":
                    return await hidController.SendKey(deviceAddress, HidKeyCode.GamepadStart, cancellationToken);

                case "steam":
                case "guide":
                    return await hidController.SendKey(deviceAddress, HidKeyCode.GamepadSteam, cancellationToken);

                case "left_stick":
                case "l3":
                    return await hidController.SendKey(deviceAddress, HidKeyCode.GamepadLeftStick, cancellationToken);

                case "right_stick":
                case "r3":
                    return await hidController.SendKey(deviceAddress, HidKeyCode.GamepadRightStick, cancellationToken);

                case "left_stick_up":
                    return await hidController.SendKey(deviceAddress, HidKeyCode.GamepadLeftStickUp, cancellationToken);

                case "left_stick_down":
                    return await hidController.SendKey(deviceAddress, HidKeyCode.GamepadLeftStickDown, cancellationToken);

                case "left_stick_left":
                    return await hidController.SendKey(deviceAddress, HidKeyCode.GamepadLeftStickLeft, cancellationToken);

                case "left_stick_right":
                    return await hidController.SendKey(deviceAddress, HidKeyCode.GamepadLeftStickRight, cancellationToken);

                case "right_stick_up":
                    return await hidController.SendKey(deviceAddress, HidKeyCode.GamepadRightStickUp, cancellationToken);

                case "right_stick_down":
                    return await hidController.SendKey(deviceAddress, HidKeyCode.GamepadRightStickDown, cancellationToken);

                case "right_stick_left":
                    return await hidController.SendKey(deviceAddress, HidKeyCode.GamepadRightStickLeft, cancellationToken);

                case "right_stick_right":
                    return await hidController.SendKey(deviceAddress, HidKeyCode.GamepadRightStickRight, cancellationToken);

                default:
                    if (Enum.TryParse<HidKeyCode>(command.NetworkPayload, true, out var keyCode))
                    {
                        return await hidController.SendKey(deviceAddress, keyCode, cancellationToken);
                    }
                    break;
            }

            logger.LogWarning("Unknown custom command payload: {Payload}", command.NetworkPayload);
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to handle custom command: {Payload}", command.NetworkPayload);
            return false;
        }
    }

    private Task<bool> HandleUnknownCommand(DeviceCommand command, CancellationToken cancellationToken)
    {
        logger.LogWarning("Unknown Steam Deck Bluetooth command type: {CommandType}", command.Type);
        return Task.FromResult(false);
    }

    private static bool IsSteamDeckDevice(BluetoothDeviceInfo device)
    {
        var name = device.Name?.ToLowerInvariant() ?? "";
        var alias = device.Alias?.ToLowerInvariant() ?? "";

        return name.Contains("steam deck") ||
               name.Contains("steamdeck") ||
               name.Contains("steam controller") ||
               alias.Contains("steam deck") ||
               alias.Contains("steamdeck") ||
               alias.Contains("steam controller") ||
               device.UuiDs.Any(uuid => IsSteamDeckServiceUuid(uuid));
    }

    private static bool IsSteamDeckServiceUuid(string uuid)
    {
        var steamDeckUuids = new[]
        {
            "00001812-0000-1000-8000-00805f9b34fb",
            "0000180f-0000-1000-8000-00805f9b34fb"
        };

        return steamDeckUuids.Any(deckUuid => string.Equals(uuid, deckUuid, StringComparison.OrdinalIgnoreCase));
    }
}