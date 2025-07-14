using Zapper.Core.Models;
using Microsoft.Extensions.Logging;

namespace Zapper.Device.Bluetooth;

public class AndroidTVBluetoothController : IBluetoothDeviceController
{
    private readonly IBluetoothHIDController _hidController;
    private readonly IBluetoothService _bluetoothService;
    private readonly ILogger<AndroidTVBluetoothController> _logger;

    public AndroidTVBluetoothController(
        IBluetoothHIDController hidController,
        IBluetoothService bluetoothService,
        ILogger<AndroidTVBluetoothController> logger)
    {
        _hidController = hidController ?? throw new ArgumentNullException(nameof(hidController));
        _bluetoothService = bluetoothService ?? throw new ArgumentNullException(nameof(bluetoothService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> SendCommandAsync(Zapper.Core.Models.Device device, DeviceCommand command, CancellationToken cancellationToken = default)
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
            // Ensure device is connected
            var isConnected = await _hidController.IsConnectedAsync(device.MacAddress, cancellationToken);
            if (!isConnected)
            {
                _logger.LogInformation("Connecting to Android TV device {DeviceName} ({Address})", device.Name, device.MacAddress);
                var connected = await _hidController.ConnectAsync(device.MacAddress, cancellationToken);
                if (!connected)
                {
                    _logger.LogError("Failed to connect to Bluetooth device {DeviceName}", device.Name);
                    return false;
                }
            }

            // Execute the command based on type
            return command.Type switch
            {
                CommandType.Power => await HandlePowerCommand(device.MacAddress, command, cancellationToken),
                CommandType.VolumeUp => await _hidController.SendKeyAsync(device.MacAddress, HIDKeyCode.VolumeUp, cancellationToken),
                CommandType.VolumeDown => await _hidController.SendKeyAsync(device.MacAddress, HIDKeyCode.VolumeDown, cancellationToken),
                CommandType.Mute => await _hidController.SendKeyAsync(device.MacAddress, HIDKeyCode.VolumeMute, cancellationToken),
                CommandType.ChannelUp => await HandleChannelUp(device.MacAddress, cancellationToken),
                CommandType.ChannelDown => await HandleChannelDown(device.MacAddress, cancellationToken),
                CommandType.Input => await HandleInputCommand(device.MacAddress, command, cancellationToken),
                CommandType.Menu => await _hidController.SendKeyAsync(device.MacAddress, HIDKeyCode.Menu, cancellationToken),
                CommandType.Back => await _hidController.SendKeyAsync(device.MacAddress, HIDKeyCode.Back, cancellationToken),
                CommandType.Home => await _hidController.SendKeyAsync(device.MacAddress, HIDKeyCode.Home, cancellationToken),
                CommandType.OK => await _hidController.SendKeyAsync(device.MacAddress, HIDKeyCode.DPadCenter, cancellationToken),
                CommandType.DirectionalUp => await _hidController.SendKeyAsync(device.MacAddress, HIDKeyCode.DPadUp, cancellationToken),
                CommandType.DirectionalDown => await _hidController.SendKeyAsync(device.MacAddress, HIDKeyCode.DPadDown, cancellationToken),
                CommandType.DirectionalLeft => await _hidController.SendKeyAsync(device.MacAddress, HIDKeyCode.DPadLeft, cancellationToken),
                CommandType.DirectionalRight => await _hidController.SendKeyAsync(device.MacAddress, HIDKeyCode.DPadRight, cancellationToken),
                CommandType.Number => await HandleNumberCommand(device.MacAddress, command, cancellationToken),
                CommandType.PlayPause => await _hidController.SendKeyAsync(device.MacAddress, HIDKeyCode.PlayPause, cancellationToken),
                CommandType.Stop => await _hidController.SendKeyAsync(device.MacAddress, HIDKeyCode.Stop, cancellationToken),
                CommandType.FastForward => await _hidController.SendKeyAsync(device.MacAddress, HIDKeyCode.FastForward, cancellationToken),
                CommandType.Rewind => await _hidController.SendKeyAsync(device.MacAddress, HIDKeyCode.Rewind, cancellationToken),
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
            // Try to get device info to test connection
            var deviceInfo = await _bluetoothService.GetDeviceAsync(device.MacAddress, cancellationToken);
            if (deviceInfo == null)
            {
                return false;
            }

            // If not connected, try to connect
            if (!deviceInfo.IsConnected)
            {
                return await _bluetoothService.ConnectDeviceAsync(device.MacAddress, cancellationToken);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to test connection to device {DeviceName}", device.Name);
            return false;
        }
    }

    public async Task<IEnumerable<string>> DiscoverPairedDevicesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var devices = await _bluetoothService.GetDevicesAsync(cancellationToken);
            return devices
                .Where(d => d.IsPaired && IsAndroidTVDevice(d))
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
        // Android TV doesn't have a direct power button via Bluetooth HID
        // Send Home button to wake up the device or show home screen
        _logger.LogInformation("Power command requested - using Home button to wake/show home screen");
        return await _hidController.SendKeyAsync(deviceAddress, HIDKeyCode.Home, cancellationToken);
    }

    private async Task<bool> HandleChannelUp(string deviceAddress, CancellationToken cancellationToken)
    {
        // For Android TV, channel up can be mapped to page up or arrow up
        return await _hidController.SendKeyAsync(deviceAddress, HIDKeyCode.PageUp, cancellationToken);
    }

    private async Task<bool> HandleChannelDown(string deviceAddress, CancellationToken cancellationToken)
    {
        // For Android TV, channel down can be mapped to page down or arrow down
        return await _hidController.SendKeyAsync(deviceAddress, HIDKeyCode.PageDown, cancellationToken);
    }

    private async Task<bool> HandleInputCommand(string deviceAddress, DeviceCommand command, CancellationToken cancellationToken)
    {
        // Input switching typically opens the input selection menu
        return await _hidController.SendKeyAsync(deviceAddress, HIDKeyCode.Menu, cancellationToken);
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
                return await _hidController.SendKeyAsync(deviceAddress, keyCode.Value, cancellationToken);
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
            // Handle text input
            if (command.NetworkPayload.StartsWith("text:"))
            {
                var text = command.NetworkPayload.Substring(5);
                return await _hidController.SendTextAsync(deviceAddress, text, cancellationToken);
            }

            // Handle specific Android TV commands
            switch (command.NetworkPayload.ToLowerInvariant())
            {
                case "netflix":
                    // Send key combination to open Netflix (example)
                    return await _hidController.SendKeySequenceAsync(deviceAddress,
                        [HIDKeyCode.Home, HIDKeyCode.N], 100, cancellationToken);
                
                case "youtube":
                    // Send key combination to open YouTube (example)
                    return await _hidController.SendKeySequenceAsync(deviceAddress,
                        [HIDKeyCode.Home, HIDKeyCode.Y], 100, cancellationToken);
                
                case "assistant":
                    // Activate Google Assistant
                    return await _hidController.SendKeyAsync(deviceAddress, HIDKeyCode.Assistant, cancellationToken);
                
                case "search":
                    // Open search
                    return await _hidController.SendKeyAsync(deviceAddress, HIDKeyCode.Search, cancellationToken);
                
                case "settings":
                    // Open settings
                    return await _hidController.SendKeyAsync(deviceAddress, HIDKeyCode.Settings, cancellationToken);
                
                default:
                    // Try to parse as HID key code
                    if (Enum.TryParse<HIDKeyCode>(command.NetworkPayload, true, out var keyCode))
                    {
                        return await _hidController.SendKeyAsync(deviceAddress, keyCode, cancellationToken);
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

    private static bool IsAndroidTVDevice(BluetoothDeviceInfo device)
    {
        // Check if device name suggests it's an Android TV
        var name = device.Name?.ToLowerInvariant() ?? "";
        var alias = device.Alias?.ToLowerInvariant() ?? "";
        
        return name.Contains("android tv") || 
               name.Contains("chromecast") || 
               name.Contains("google tv") ||
               alias.Contains("android tv") || 
               alias.Contains("chromecast") || 
               alias.Contains("google tv") ||
               // Check for Android TV service UUIDs if available
               device.UUIDs.Any(uuid => IsAndroidTVServiceUuid(uuid));
    }

    private static bool IsAndroidTVServiceUuid(string uuid)
    {
        // Common Android TV Bluetooth service UUIDs
        var androidTvUuids = new[]
        {
            "0000fef3-0000-1000-8000-00805f9b34fb", // Google specific service
            "0000180f-0000-1000-8000-00805f9b34fb", // Battery Service (common on Android TV remotes)
            "00001812-0000-1000-8000-00805f9b34fb"  // Human Interface Device
        };

        return androidTvUuids.Any(tvUuid => string.Equals(uuid, tvUuid, StringComparison.OrdinalIgnoreCase));
    }
}