using Zapper.Core.Models;
using Microsoft.Extensions.Logging;

namespace Zapper.Device.Bluetooth;

public class AndroidTvBluetoothController : IBluetoothDeviceController
{
    private readonly IBluetoothHidController _hidController;
    private readonly IBluetoothService _bluetoothService;
    private readonly ILogger<AndroidTvBluetoothController> _logger;

    public AndroidTvBluetoothController(IBluetoothHidController hidController, IBluetoothService bluetoothService, ILogger<AndroidTvBluetoothController> logger)
    {
        ArgumentNullException.ThrowIfNull(hidController);
        ArgumentNullException.ThrowIfNull(bluetoothService);
        ArgumentNullException.ThrowIfNull(logger);

        _hidController = hidController;
        _bluetoothService = bluetoothService;
        _logger = logger;
    }
    public async Task<bool> SendCommand(Zapper.Core.Models.Device device, DeviceCommand command, CancellationToken cancellationToken = default)
    {
        if (device.ConnectionType != ConnectionType.Bluetooth)
        {
            _logger.LogWarning("Device {DeviceName} is not a Bluetooth device", device.Name);
            return false;
        }

        if (string.IsNullOrEmpty(device.MacAddress))
        {
            _logger.LogWarning("Device {DeviceName} has no Bluetooth MAC address configured", device.Name);
            return false;
        }

        try
        {
            var isConnected = await _hidController.IsConnected(device.MacAddress, cancellationToken);
            if (!isConnected)
            {
                _logger.LogInformation("Connecting to Android TV device {DeviceName} ({Address})", device.Name, device.MacAddress);
                var connected = await _hidController.Connect(device.MacAddress, cancellationToken);
                if (!connected)
                {
                    _logger.LogError("Failed to connect to Bluetooth device {DeviceName}", device.Name);
                    return false;
                }
            }

            return command.Type switch
            {
                CommandType.Power => await HandlePowerCommand(device.MacAddress, command, cancellationToken),
                CommandType.VolumeUp => await _hidController.SendKey(device.MacAddress, HidKeyCode.VolumeUp, cancellationToken),
                CommandType.VolumeDown => await _hidController.SendKey(device.MacAddress, HidKeyCode.VolumeDown, cancellationToken),
                CommandType.Mute => await _hidController.SendKey(device.MacAddress, HidKeyCode.VolumeMute, cancellationToken),
                CommandType.ChannelUp => await HandleChannelUp(device.MacAddress, cancellationToken),
                CommandType.ChannelDown => await HandleChannelDown(device.MacAddress, cancellationToken),
                CommandType.Input => await HandleInputCommand(device.MacAddress, command, cancellationToken),
                CommandType.Menu => await _hidController.SendKey(device.MacAddress, HidKeyCode.Menu, cancellationToken),
                CommandType.Back => await _hidController.SendKey(device.MacAddress, HidKeyCode.Back, cancellationToken),
                CommandType.Home => await _hidController.SendKey(device.MacAddress, HidKeyCode.Home, cancellationToken),
                CommandType.Ok => await _hidController.SendKey(device.MacAddress, HidKeyCode.DPadCenter, cancellationToken),
                CommandType.DirectionalUp => await _hidController.SendKey(device.MacAddress, HidKeyCode.DPadUp, cancellationToken),
                CommandType.DirectionalDown => await _hidController.SendKey(device.MacAddress, HidKeyCode.DPadDown, cancellationToken),
                CommandType.DirectionalLeft => await _hidController.SendKey(device.MacAddress, HidKeyCode.DPadLeft, cancellationToken),
                CommandType.DirectionalRight => await _hidController.SendKey(device.MacAddress, HidKeyCode.DPadRight, cancellationToken),
                CommandType.Number => await HandleNumberCommand(device.MacAddress, command, cancellationToken),
                CommandType.PlayPause => await _hidController.SendKey(device.MacAddress, HidKeyCode.PlayPause, cancellationToken),
                CommandType.Stop => await _hidController.SendKey(device.MacAddress, HidKeyCode.Stop, cancellationToken),
                CommandType.FastForward => await _hidController.SendKey(device.MacAddress, HidKeyCode.FastForward, cancellationToken),
                CommandType.Rewind => await _hidController.SendKey(device.MacAddress, HidKeyCode.Rewind, cancellationToken),
                CommandType.Custom => await HandleCustomCommand(device.MacAddress, command, cancellationToken),
                _ => await HandleUnknownCommand(command, cancellationToken)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send Bluetooth command {CommandType} to device {DeviceName}",
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
            var deviceInfo = await _bluetoothService.GetDevice(device.MacAddress, cancellationToken);
            if (deviceInfo == null)
            {
                return false;
            }

            if (!deviceInfo.IsConnected)
            {
                return await _bluetoothService.ConnectDevice(device.MacAddress, cancellationToken);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to test connection to device {DeviceName}", device.Name);
            return false;
        }
    }

    public async Task<IEnumerable<string>> DiscoverPairedDevices(CancellationToken cancellationToken = default)
    {
        try
        {
            var devices = await _bluetoothService.GetDevices(cancellationToken);
            return devices
                .Where(d => d.IsPaired && IsAndroidTvDevice(d))
                .Select(d => d.Address)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to discover paired Android TV devices");
            return [];
        }
    }

    private async Task<bool> HandlePowerCommand(string deviceAddress, DeviceCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Power command requested - using Home button to wake/show home screen");
        return await _hidController.SendKey(deviceAddress, HidKeyCode.Home, cancellationToken);
    }

    private async Task<bool> HandleChannelUp(string deviceAddress, CancellationToken cancellationToken)
    {
        return await _hidController.SendKey(deviceAddress, HidKeyCode.PageUp, cancellationToken);
    }

    private async Task<bool> HandleChannelDown(string deviceAddress, CancellationToken cancellationToken)
    {
        return await _hidController.SendKey(deviceAddress, HidKeyCode.PageDown, cancellationToken);
    }

    private async Task<bool> HandleInputCommand(string deviceAddress, DeviceCommand command, CancellationToken cancellationToken)
    {
        return await _hidController.SendKey(deviceAddress, HidKeyCode.Menu, cancellationToken);
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
                return await _hidController.SendKey(deviceAddress, keyCode.Value, cancellationToken);
            }
        }

        _logger.LogWarning("Invalid number for number command: {Payload}", command.NetworkPayload);
        return false;
    }

    private async Task<bool> HandleCustomCommand(string deviceAddress, DeviceCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(command.NetworkPayload))
        {
            _logger.LogWarning("Custom command has no payload");
            return false;
        }

        try
        {
            if (command.NetworkPayload.StartsWith("text:"))
            {
                var text = command.NetworkPayload.Substring(5);
                return await _hidController.SendText(deviceAddress, text, cancellationToken);
            }

            switch (command.NetworkPayload.ToLowerInvariant())
            {
                case "netflix":
                    return await _hidController.SendKeySequence(deviceAddress,
                        [HidKeyCode.Home, HidKeyCode.N], 100, cancellationToken);

                case "youtube":
                    return await _hidController.SendKeySequence(deviceAddress,
                        [HidKeyCode.Home, HidKeyCode.Y], 100, cancellationToken);

                case "assistant":
                    return await _hidController.SendKey(deviceAddress, HidKeyCode.Assistant, cancellationToken);

                case "search":
                    return await _hidController.SendKey(deviceAddress, HidKeyCode.Search, cancellationToken);

                case "settings":
                    return await _hidController.SendKey(deviceAddress, HidKeyCode.Settings, cancellationToken);

                default:
                    if (Enum.TryParse<HidKeyCode>(command.NetworkPayload, true, out var keyCode))
                    {
                        return await _hidController.SendKey(deviceAddress, keyCode, cancellationToken);
                    }
                    break;
            }

            _logger.LogWarning("Unknown custom command payload: {Payload}", command.NetworkPayload);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle custom command: {Payload}", command.NetworkPayload);
            return false;
        }
    }

    private Task<bool> HandleUnknownCommand(DeviceCommand command, CancellationToken cancellationToken)
    {
        _logger.LogWarning("Unknown Bluetooth command type: {CommandType}", command.Type);
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