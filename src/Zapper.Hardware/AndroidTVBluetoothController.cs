using Zapper.Core.Models;
using Microsoft.Extensions.Logging;

namespace Zapper.Hardware;

public class AndroidTVBluetoothController : IBluetoothDeviceController
{
    private readonly IBluetoothHIDController _hidController;
    private readonly ILogger<AndroidTVBluetoothController> _logger;

    public AndroidTVBluetoothController(IBluetoothHIDController hidController, ILogger<AndroidTVBluetoothController> logger)
    {
        _hidController = hidController;
        _logger = logger;
    }

    public async Task<bool> SendCommandAsync(Device device, DeviceCommand command, CancellationToken cancellationToken = default)
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
            // Connect if not already connected
            if (!_hidController.IsConnected || _hidController.ConnectedDeviceId != device.MacAddress)
            {
                var connected = await _hidController.ConnectToDeviceAsync(device.MacAddress, cancellationToken);
                if (!connected)
                {
                    _logger.LogError("Failed to connect to Bluetooth device {DeviceName}", device.Name);
                    return false;
                }
            }

            // Execute the command based on type
            return command.Type switch
            {
                CommandType.Power => await HandlePowerCommand(command, cancellationToken),
                CommandType.VolumeUp => await _hidController.SendKeyEventAsync(HIDKeyCode.VolumeUp, true, cancellationToken),
                CommandType.VolumeDown => await _hidController.SendKeyEventAsync(HIDKeyCode.VolumeDown, true, cancellationToken),
                CommandType.Mute => await _hidController.SendKeyEventAsync(HIDKeyCode.VolumeMute, true, cancellationToken),
                CommandType.ChannelUp => await HandleChannelUp(cancellationToken),
                CommandType.ChannelDown => await HandleChannelDown(cancellationToken),
                CommandType.Input => await HandleInputCommand(command, cancellationToken),
                CommandType.Menu => await _hidController.SendKeyEventAsync(HIDKeyCode.Menu, true, cancellationToken),
                CommandType.Back => await _hidController.SendKeyEventAsync(HIDKeyCode.Back, true, cancellationToken),
                CommandType.Home => await _hidController.SendKeyEventAsync(HIDKeyCode.Home, true, cancellationToken),
                CommandType.OK => await _hidController.SendKeyEventAsync(HIDKeyCode.DPadCenter, true, cancellationToken),
                CommandType.DirectionalUp => await _hidController.SendKeyEventAsync(HIDKeyCode.DPadUp, true, cancellationToken),
                CommandType.DirectionalDown => await _hidController.SendKeyEventAsync(HIDKeyCode.DPadDown, true, cancellationToken),
                CommandType.DirectionalLeft => await _hidController.SendKeyEventAsync(HIDKeyCode.DPadLeft, true, cancellationToken),
                CommandType.DirectionalRight => await _hidController.SendKeyEventAsync(HIDKeyCode.DPadRight, true, cancellationToken),
                CommandType.Number => await HandleNumberCommand(command, cancellationToken),
                CommandType.PlayPause => await _hidController.SendKeyEventAsync(HIDKeyCode.PlayPause, true, cancellationToken),
                CommandType.Stop => await _hidController.SendKeyEventAsync(HIDKeyCode.Stop, true, cancellationToken),
                CommandType.FastForward => await _hidController.SendKeyEventAsync(HIDKeyCode.FastForward, true, cancellationToken),
                CommandType.Rewind => await _hidController.SendKeyEventAsync(HIDKeyCode.Rewind, true, cancellationToken),
                CommandType.Custom => await HandleCustomCommand(command, cancellationToken),
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

    private async Task<bool> HandlePowerCommand(DeviceCommand command, CancellationToken cancellationToken)
    {
        // Android TV doesn't have a direct power button via Bluetooth HID
        // Use Home button as alternative or custom implementation
        _logger.LogInformation("Power command requested - using Home button as fallback");
        return await _hidController.SendKeyEventAsync(HIDKeyCode.Home, true, cancellationToken);
    }

    private async Task<bool> HandleChannelUp(CancellationToken cancellationToken)
    {
        // Channel up can be simulated with arrow up
        return await _hidController.SendKeyEventAsync(HIDKeyCode.ArrowUp, true, cancellationToken);
    }

    private async Task<bool> HandleChannelDown(CancellationToken cancellationToken)
    {
        // Channel down can be simulated with arrow down
        return await _hidController.SendKeyEventAsync(HIDKeyCode.ArrowDown, true, cancellationToken);
    }

    private async Task<bool> HandleInputCommand(DeviceCommand command, CancellationToken cancellationToken)
    {
        // Input switching via Menu or custom key sequence
        return await _hidController.SendKeyEventAsync(HIDKeyCode.Menu, true, cancellationToken);
    }

    private async Task<bool> HandleNumberCommand(DeviceCommand command, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(command.NetworkPayload) && int.TryParse(command.NetworkPayload, out var number))
        {
            var keyCode = number switch
            {
                0 => HIDKeyCode.Number0,
                1 => HIDKeyCode.Number1,
                2 => HIDKeyCode.Number2,
                3 => HIDKeyCode.Number3,
                4 => HIDKeyCode.Number4,
                5 => HIDKeyCode.Number5,
                6 => HIDKeyCode.Number6,
                7 => HIDKeyCode.Number7,
                8 => HIDKeyCode.Number8,
                9 => HIDKeyCode.Number9,
                _ => (HIDKeyCode?)null
            };

            if (keyCode.HasValue)
            {
                return await _hidController.SendKeyEventAsync(keyCode.Value, true, cancellationToken);
            }
        }

        _logger.LogWarning("Invalid number for number command: {Payload}", command.NetworkPayload);
        return false;
    }

    private async Task<bool> HandleCustomCommand(DeviceCommand command, CancellationToken cancellationToken)
    {
        // Handle custom commands based on NetworkPayload
        if (!string.IsNullOrEmpty(command.NetworkPayload))
        {
            // Check if it's a keyboard text command
            if (command.NetworkPayload.StartsWith("text:"))
            {
                var text = command.NetworkPayload.Substring(5);
                return await _hidController.SendKeyboardTextAsync(text, cancellationToken);
            }

            // Check if it's a mouse command
            if (command.NetworkPayload.StartsWith("mouse:"))
            {
                return await HandleMouseCommand(command.NetworkPayload, cancellationToken);
            }

            // Check if it's a specific key code
            if (Enum.TryParse<HIDKeyCode>(command.NetworkPayload, true, out var keyCode))
            {
                return await _hidController.SendKeyEventAsync(keyCode, true, cancellationToken);
            }
        }

        _logger.LogWarning("Unknown custom command payload: {Payload}", command.NetworkPayload);
        return false;
    }

    private async Task<bool> HandleMouseCommand(string payload, CancellationToken cancellationToken)
    {
        try
        {
            // Parse mouse command: "mouse:x,y,leftClick,rightClick"
            var parts = payload.Substring(6).Split(',');
            if (parts.Length >= 2)
            {
                var deltaX = int.Parse(parts[0]);
                var deltaY = int.Parse(parts[1]);
                var leftClick = parts.Length > 2 && bool.Parse(parts[2]);
                var rightClick = parts.Length > 3 && bool.Parse(parts[3]);

                return await _hidController.SendMouseEventAsync(deltaX, deltaY, leftClick, rightClick, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse mouse command: {Payload}", payload);
        }

        return false;
    }

    private async Task<bool> HandleUnknownCommand(DeviceCommand command, CancellationToken cancellationToken)
    {
        _logger.LogWarning("Unknown Bluetooth command type: {CommandType}", command.Type);
        return false;
    }

    public Task<bool> TestConnectionAsync(Device device, CancellationToken cancellationToken = default)
    {
        // For Bluetooth devices, testing connection means trying to connect
        return SendCommandAsync(device, new DeviceCommand 
        { 
            Type = CommandType.Custom, 
            NetworkPayload = "test"
        }, cancellationToken);
    }

    public async Task<IEnumerable<string>> DiscoverPairedDevicesAsync(CancellationToken cancellationToken = default)
    {
        // Return list of paired Bluetooth devices
        return await _hidController.GetPairedDevicesAsync(cancellationToken);
    }
}