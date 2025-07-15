using Zapper.Core.Models;
using Microsoft.Extensions.Logging;

namespace Zapper.Device.Bluetooth;

public class AppleTvBluetoothController(
    IBluetoothHidController hidController,
    IBluetoothService bluetoothService,
    ILogger<AppleTvBluetoothController> logger) : IBluetoothDeviceController
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
                logger.LogInformation("Connecting to Apple TV device {DeviceName} ({Address})", device.Name, device.MacAddress);
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
                CommandType.VolumeUp => await hidController.SendKey(device.MacAddress, HidKeyCode.VolumeUp, cancellationToken),
                CommandType.VolumeDown => await hidController.SendKey(device.MacAddress, HidKeyCode.VolumeDown, cancellationToken),
                CommandType.Mute => await hidController.SendKey(device.MacAddress, HidKeyCode.VolumeMute, cancellationToken),
                CommandType.ChannelUp => await HandleChannelUp(device.MacAddress, cancellationToken),
                CommandType.ChannelDown => await HandleChannelDown(device.MacAddress, cancellationToken),
                CommandType.Input => await HandleInputCommand(device.MacAddress, command, cancellationToken),
                CommandType.Menu => await hidController.SendKey(device.MacAddress, HidKeyCode.Menu, cancellationToken),
                CommandType.Back => await hidController.SendKey(device.MacAddress, HidKeyCode.Back, cancellationToken),
                CommandType.Home => await hidController.SendKey(device.MacAddress, HidKeyCode.Home, cancellationToken),
                CommandType.Ok => await hidController.SendKey(device.MacAddress, HidKeyCode.DPadCenter, cancellationToken),
                CommandType.DirectionalUp => await hidController.SendKey(device.MacAddress, HidKeyCode.DPadUp, cancellationToken),
                CommandType.DirectionalDown => await hidController.SendKey(device.MacAddress, HidKeyCode.DPadDown, cancellationToken),
                CommandType.DirectionalLeft => await hidController.SendKey(device.MacAddress, HidKeyCode.DPadLeft, cancellationToken),
                CommandType.DirectionalRight => await hidController.SendKey(device.MacAddress, HidKeyCode.DPadRight, cancellationToken),
                CommandType.Number => await HandleNumberCommand(device.MacAddress, command, cancellationToken),
                CommandType.PlayPause => await hidController.SendKey(device.MacAddress, HidKeyCode.PlayPause, cancellationToken),
                CommandType.Stop => await hidController.SendKey(device.MacAddress, HidKeyCode.Stop, cancellationToken),
                CommandType.FastForward => await hidController.SendKey(device.MacAddress, HidKeyCode.FastForward, cancellationToken),
                CommandType.Rewind => await hidController.SendKey(device.MacAddress, HidKeyCode.Rewind, cancellationToken),
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
                .Where(d => d.IsPaired && IsAppleTvDevice(d))
                .Select(d => d.Address)
                .ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to discover paired Apple TV devices");
            return [];
        }
    }

    private async Task<bool> HandlePowerCommand(string deviceAddress, DeviceCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Power command requested - using Menu button to wake/sleep Apple TV");
        return await hidController.SendKey(deviceAddress, HidKeyCode.Menu, cancellationToken);
    }

    private async Task<bool> HandleChannelUp(string deviceAddress, CancellationToken cancellationToken)
    {
        return await hidController.SendKey(deviceAddress, HidKeyCode.DPadRight, cancellationToken);
    }

    private async Task<bool> HandleChannelDown(string deviceAddress, CancellationToken cancellationToken)
    {
        return await hidController.SendKey(deviceAddress, HidKeyCode.DPadLeft, cancellationToken);
    }

    private async Task<bool> HandleInputCommand(string deviceAddress, DeviceCommand command, CancellationToken cancellationToken)
    {
        return await hidController.SendKeySequence(deviceAddress,
            [HidKeyCode.Home, HidKeyCode.Home], 200, cancellationToken);
    }

    private async Task<bool> HandleNumberCommand(string deviceAddress, DeviceCommand command, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(command.NetworkPayload) && int.TryParse(command.NetworkPayload, out var number))
        {
            var keyCode = number switch
            {
                0 => HidKeyCode.Key0,
                1 => HidKeyCode.Key1,
                2 => HidKeyCode.Key2,
                3 => HidKeyCode.Key3,
                4 => HidKeyCode.Key4,
                5 => HidKeyCode.Key5,
                6 => HidKeyCode.Key6,
                7 => HidKeyCode.Key7,
                8 => HidKeyCode.Key8,
                9 => HidKeyCode.Key9,
                _ => (HidKeyCode?)null
            };

            if (keyCode.HasValue)
            {
                return await hidController.SendKey(deviceAddress, keyCode.Value, cancellationToken);
            }
        }

        logger.LogWarning("Invalid number for number command: {Payload}", command.NetworkPayload);
        return false;
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
            if (command.NetworkPayload.StartsWith("text:"))
            {
                var text = command.NetworkPayload.Substring(5);
                return await hidController.SendText(deviceAddress, text, cancellationToken);
            }

            switch (command.NetworkPayload.ToLowerInvariant())
            {
                case "siri":
                    return await hidController.SendKey(deviceAddress, HidKeyCode.Assistant, cancellationToken);

                case "app_switcher":
                    return await hidController.SendKeySequence(deviceAddress,
                        [HidKeyCode.Home, HidKeyCode.Home], 200, cancellationToken);

                case "control_center":
                    return await hidController.SendKey(deviceAddress, HidKeyCode.Menu, cancellationToken);

                case "netflix":
                    return await hidController.SendKeySequence(deviceAddress,
                        [HidKeyCode.Home, HidKeyCode.N], 100, cancellationToken);

                case "disney":
                case "disney+":
                    return await hidController.SendKeySequence(deviceAddress,
                        [HidKeyCode.Home, HidKeyCode.D], 100, cancellationToken);

                case "youtube":
                    return await hidController.SendKeySequence(deviceAddress,
                        [HidKeyCode.Home, HidKeyCode.Y], 100, cancellationToken);

                case "search":
                    return await hidController.SendKey(deviceAddress, HidKeyCode.Search, cancellationToken);

                case "settings":
                    return await hidController.SendKey(deviceAddress, HidKeyCode.Settings, cancellationToken);

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
        logger.LogWarning("Unknown Bluetooth command type: {CommandType}", command.Type);
        return Task.FromResult(false);
    }

    private static bool IsAppleTvDevice(BluetoothDeviceInfo device)
    {
        var name = device.Name?.ToLowerInvariant() ?? "";
        var alias = device.Alias?.ToLowerInvariant() ?? "";

        return name.Contains("apple tv") ||
               name.Contains("appletv") ||
               name.Contains("siri remote") ||
               name.Contains("apple remote") ||
               alias.Contains("apple tv") ||
               alias.Contains("appletv") ||
               alias.Contains("siri remote") ||
               alias.Contains("apple remote") ||
               device.UuiDs.Any(uuid => IsAppleTvServiceUuid(uuid));
    }

    private static bool IsAppleTvServiceUuid(string uuid)
    {
        var appleTvUuids = new[]
        {
            "0000180f-0000-1000-8000-00805f9b34fb",
            "00001812-0000-1000-8000-00805f9b34fb",
            "0000180a-0000-1000-8000-00805f9b34fb",
            "89d3502b-0f36-433a-8ef4-c502ad55f8dc",
            "7905f431-b5ce-4e99-a40f-4b1e122d00d0"
        };

        return appleTvUuids.Any(tvUuid => string.Equals(uuid, tvUuid, StringComparison.OrdinalIgnoreCase));
    }
}