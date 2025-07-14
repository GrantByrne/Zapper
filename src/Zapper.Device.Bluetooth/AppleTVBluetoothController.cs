using Zapper.Core.Models;
using Microsoft.Extensions.Logging;

namespace Zapper.Device.Bluetooth;

public class AppleTVBluetoothController(IBluetoothHIDController hidController, IBluetoothService bluetoothService, ILogger<AppleTVBluetoothController> logger) : IBluetoothDeviceController
{

    public async Task<bool> SendCommandAsync(Zapper.Core.Models.Device device, DeviceCommand command, CancellationToken cancellationToken = default)
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
            var isConnected = await hidController.IsConnectedAsync(device.MacAddress, cancellationToken);
            if (!isConnected)
            {
                logger.LogInformation("Connecting to Apple TV device {DeviceName} ({Address})", device.Name, device.MacAddress);
                var connected = await hidController.ConnectAsync(device.MacAddress, cancellationToken);
                if (!connected)
                {
                    logger.LogError("Failed to connect to Bluetooth device {DeviceName}", device.Name);
                    return false;
                }
            }

            return command.Type switch
            {
                CommandType.Power => await HandlePowerCommand(device.MacAddress, command, cancellationToken),
                CommandType.VolumeUp => await hidController.SendKeyAsync(device.MacAddress, HIDKeyCode.VolumeUp, cancellationToken),
                CommandType.VolumeDown => await hidController.SendKeyAsync(device.MacAddress, HIDKeyCode.VolumeDown, cancellationToken),
                CommandType.Mute => await hidController.SendKeyAsync(device.MacAddress, HIDKeyCode.VolumeMute, cancellationToken),
                CommandType.ChannelUp => await HandleChannelUp(device.MacAddress, cancellationToken),
                CommandType.ChannelDown => await HandleChannelDown(device.MacAddress, cancellationToken),
                CommandType.Input => await HandleInputCommand(device.MacAddress, command, cancellationToken),
                CommandType.Menu => await hidController.SendKeyAsync(device.MacAddress, HIDKeyCode.Menu, cancellationToken),
                CommandType.Back => await hidController.SendKeyAsync(device.MacAddress, HIDKeyCode.Back, cancellationToken),
                CommandType.Home => await hidController.SendKeyAsync(device.MacAddress, HIDKeyCode.Home, cancellationToken),
                CommandType.OK => await hidController.SendKeyAsync(device.MacAddress, HIDKeyCode.DPadCenter, cancellationToken),
                CommandType.DirectionalUp => await hidController.SendKeyAsync(device.MacAddress, HIDKeyCode.DPadUp, cancellationToken),
                CommandType.DirectionalDown => await hidController.SendKeyAsync(device.MacAddress, HIDKeyCode.DPadDown, cancellationToken),
                CommandType.DirectionalLeft => await hidController.SendKeyAsync(device.MacAddress, HIDKeyCode.DPadLeft, cancellationToken),
                CommandType.DirectionalRight => await hidController.SendKeyAsync(device.MacAddress, HIDKeyCode.DPadRight, cancellationToken),
                CommandType.Number => await HandleNumberCommand(device.MacAddress, command, cancellationToken),
                CommandType.PlayPause => await hidController.SendKeyAsync(device.MacAddress, HIDKeyCode.PlayPause, cancellationToken),
                CommandType.Stop => await hidController.SendKeyAsync(device.MacAddress, HIDKeyCode.Stop, cancellationToken),
                CommandType.FastForward => await hidController.SendKeyAsync(device.MacAddress, HIDKeyCode.FastForward, cancellationToken),
                CommandType.Rewind => await hidController.SendKeyAsync(device.MacAddress, HIDKeyCode.Rewind, cancellationToken),
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

    public async Task<bool> TestConnectionAsync(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default)
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
            var deviceInfo = await bluetoothService.GetDeviceAsync(device.MacAddress, cancellationToken);
            if (deviceInfo == null)
            {
                return false;
            }

            if (!deviceInfo.IsConnected)
            {
                return await bluetoothService.ConnectDeviceAsync(device.MacAddress, cancellationToken);
            }

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to test connection to device {DeviceName}", device.Name);
            return false;
        }
    }

    public async Task<IEnumerable<string>> DiscoverPairedDevicesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var devices = await bluetoothService.GetDevicesAsync(cancellationToken);
            return devices
                .Where(d => d.IsPaired && IsAppleTVDevice(d))
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
        return await hidController.SendKeyAsync(deviceAddress, HIDKeyCode.Menu, cancellationToken);
    }

    private async Task<bool> HandleChannelUp(string deviceAddress, CancellationToken cancellationToken)
    {
        return await hidController.SendKeyAsync(deviceAddress, HIDKeyCode.DPadRight, cancellationToken);
    }

    private async Task<bool> HandleChannelDown(string deviceAddress, CancellationToken cancellationToken)
    {
        return await hidController.SendKeyAsync(deviceAddress, HIDKeyCode.DPadLeft, cancellationToken);
    }

    private async Task<bool> HandleInputCommand(string deviceAddress, DeviceCommand command, CancellationToken cancellationToken)
    {
        return await hidController.SendKeySequenceAsync(deviceAddress,
            [HIDKeyCode.Home, HIDKeyCode.Home], 200, cancellationToken);
    }

    private async Task<bool> HandleNumberCommand(string deviceAddress, DeviceCommand command, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(command.NetworkPayload) && int.TryParse(command.NetworkPayload, out var number))
        {
            var keyCode = number switch
            {
                0 => HIDKeyCode.Key0,
                1 => HIDKeyCode.Key1,
                2 => HIDKeyCode.Key2,
                3 => HIDKeyCode.Key3,
                4 => HIDKeyCode.Key4,
                5 => HIDKeyCode.Key5,
                6 => HIDKeyCode.Key6,
                7 => HIDKeyCode.Key7,
                8 => HIDKeyCode.Key8,
                9 => HIDKeyCode.Key9,
                _ => (HIDKeyCode?)null
            };

            if (keyCode.HasValue)
            {
                return await hidController.SendKeyAsync(deviceAddress, keyCode.Value, cancellationToken);
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
                return await hidController.SendTextAsync(deviceAddress, text, cancellationToken);
            }

            switch (command.NetworkPayload.ToLowerInvariant())
            {
                case "siri":
                    return await hidController.SendKeyAsync(deviceAddress, HIDKeyCode.Assistant, cancellationToken);
                
                case "app_switcher":
                    return await hidController.SendKeySequenceAsync(deviceAddress,
                        [HIDKeyCode.Home, HIDKeyCode.Home], 200, cancellationToken);
                
                case "control_center":
                    return await hidController.SendKeyAsync(deviceAddress, HIDKeyCode.Menu, cancellationToken);
                
                case "netflix":
                    return await hidController.SendKeySequenceAsync(deviceAddress,
                        [HIDKeyCode.Home, HIDKeyCode.N], 100, cancellationToken);
                
                case "disney":
                case "disney+":
                    return await hidController.SendKeySequenceAsync(deviceAddress,
                        [HIDKeyCode.Home, HIDKeyCode.D], 100, cancellationToken);
                
                case "youtube":
                    return await hidController.SendKeySequenceAsync(deviceAddress,
                        [HIDKeyCode.Home, HIDKeyCode.Y], 100, cancellationToken);
                
                case "search":
                    return await hidController.SendKeyAsync(deviceAddress, HIDKeyCode.Search, cancellationToken);
                
                case "settings":
                    return await hidController.SendKeyAsync(deviceAddress, HIDKeyCode.Settings, cancellationToken);
                
                default:
                    if (Enum.TryParse<HIDKeyCode>(command.NetworkPayload, true, out var keyCode))
                    {
                        return await hidController.SendKeyAsync(deviceAddress, keyCode, cancellationToken);
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

    private static bool IsAppleTVDevice(BluetoothDeviceInfo device)
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
               device.UUIDs.Any(uuid => IsAppleTVServiceUuid(uuid));
    }

    private static bool IsAppleTVServiceUuid(string uuid)
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