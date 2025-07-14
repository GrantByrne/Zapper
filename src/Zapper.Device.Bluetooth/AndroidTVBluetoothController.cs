using Zapper.Core.Models;
using Microsoft.Extensions.Logging;

namespace Zapper.Device.Bluetooth;

public class AndroidTvBluetoothController(IBluetoothHidController hidController, IBluetoothService bluetoothService, ILogger<AndroidTvBluetoothController> logger) : IBluetoothDeviceController
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
                logger.LogInformation("Connecting to Android TV device {DeviceName} ({Address})", device.Name, device.MacAddress);
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
                CommandType.VolumeUp => await hidController.SendKeyAsync(device.MacAddress, HidKeyCode.VolumeUp, cancellationToken),
                CommandType.VolumeDown => await hidController.SendKeyAsync(device.MacAddress, HidKeyCode.VolumeDown, cancellationToken),
                CommandType.Mute => await hidController.SendKeyAsync(device.MacAddress, HidKeyCode.VolumeMute, cancellationToken),
                CommandType.ChannelUp => await HandleChannelUp(device.MacAddress, cancellationToken),
                CommandType.ChannelDown => await HandleChannelDown(device.MacAddress, cancellationToken),
                CommandType.Input => await HandleInputCommand(device.MacAddress, command, cancellationToken),
                CommandType.Menu => await hidController.SendKeyAsync(device.MacAddress, HidKeyCode.Menu, cancellationToken),
                CommandType.Back => await hidController.SendKeyAsync(device.MacAddress, HidKeyCode.Back, cancellationToken),
                CommandType.Home => await hidController.SendKeyAsync(device.MacAddress, HidKeyCode.Home, cancellationToken),
                CommandType.Ok => await hidController.SendKeyAsync(device.MacAddress, HidKeyCode.DPadCenter, cancellationToken),
                CommandType.DirectionalUp => await hidController.SendKeyAsync(device.MacAddress, HidKeyCode.DPadUp, cancellationToken),
                CommandType.DirectionalDown => await hidController.SendKeyAsync(device.MacAddress, HidKeyCode.DPadDown, cancellationToken),
                CommandType.DirectionalLeft => await hidController.SendKeyAsync(device.MacAddress, HidKeyCode.DPadLeft, cancellationToken),
                CommandType.DirectionalRight => await hidController.SendKeyAsync(device.MacAddress, HidKeyCode.DPadRight, cancellationToken),
                CommandType.Number => await HandleNumberCommand(device.MacAddress, command, cancellationToken),
                CommandType.PlayPause => await hidController.SendKeyAsync(device.MacAddress, HidKeyCode.PlayPause, cancellationToken),
                CommandType.Stop => await hidController.SendKeyAsync(device.MacAddress, HidKeyCode.Stop, cancellationToken),
                CommandType.FastForward => await hidController.SendKeyAsync(device.MacAddress, HidKeyCode.FastForward, cancellationToken),
                CommandType.Rewind => await hidController.SendKeyAsync(device.MacAddress, HidKeyCode.Rewind, cancellationToken),
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
                .Where(d => d.IsPaired && IsAndroidTvDevice(d))
                .Select(d => d.Address)
                .ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to discover paired Android TV devices");
            return [];
        }
    }

    private async Task<bool> HandlePowerCommand(string deviceAddress, DeviceCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Power command requested - using Home button to wake/show home screen");
        return await hidController.SendKeyAsync(deviceAddress, HidKeyCode.Home, cancellationToken);
    }

    private async Task<bool> HandleChannelUp(string deviceAddress, CancellationToken cancellationToken)
    {
        return await hidController.SendKeyAsync(deviceAddress, HidKeyCode.PageUp, cancellationToken);
    }

    private async Task<bool> HandleChannelDown(string deviceAddress, CancellationToken cancellationToken)
    {
        return await hidController.SendKeyAsync(deviceAddress, HidKeyCode.PageDown, cancellationToken);
    }

    private async Task<bool> HandleInputCommand(string deviceAddress, DeviceCommand command, CancellationToken cancellationToken)
    {
        return await hidController.SendKeyAsync(deviceAddress, HidKeyCode.Menu, cancellationToken);
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
                case "netflix":
                    return await hidController.SendKeySequenceAsync(deviceAddress,
                        [HidKeyCode.Home, HidKeyCode.N], 100, cancellationToken);
                
                case "youtube":
                    return await hidController.SendKeySequenceAsync(deviceAddress,
                        [HidKeyCode.Home, HidKeyCode.Y], 100, cancellationToken);
                
                case "assistant":
                    return await hidController.SendKeyAsync(deviceAddress, HidKeyCode.Assistant, cancellationToken);
                
                case "search":
                    return await hidController.SendKeyAsync(deviceAddress, HidKeyCode.Search, cancellationToken);
                
                case "settings":
                    return await hidController.SendKeyAsync(deviceAddress, HidKeyCode.Settings, cancellationToken);
                
                default:
                    if (Enum.TryParse<HidKeyCode>(command.NetworkPayload, true, out var keyCode))
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

    private static bool IsAndroidTvDevice(BluetoothDeviceInfo device)
    {
        var name = device.Name?.ToLowerInvariant() ?? "";
        var alias = device.Alias?.ToLowerInvariant() ?? "";
        
        return name.Contains("android tv") || 
               name.Contains("chromecast") || 
               name.Contains("google tv") ||
               alias.Contains("android tv") || 
               alias.Contains("chromecast") || 
               alias.Contains("google tv") ||
               device.UuiDs.Any(uuid => IsAndroidTvServiceUuid(uuid));
    }

    private static bool IsAndroidTvServiceUuid(string uuid)
    {
        var androidTvUuids = new[]
        {
            "0000fef3-0000-1000-8000-00805f9b34fb",
            "0000180f-0000-1000-8000-00805f9b34fb",
            "00001812-0000-1000-8000-00805f9b34fb"
        };

        return androidTvUuids.Any(tvUuid => string.Equals(uuid, tvUuid, StringComparison.OrdinalIgnoreCase));
    }
}