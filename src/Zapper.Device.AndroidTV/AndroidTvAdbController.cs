using Microsoft.Extensions.Logging;
using Zapper.Core.Interfaces;
using Zapper.Core.Models;

namespace Zapper.Device.AndroidTV;

public class AndroidTvAdbController(IAdbClient adbClient, ILogger<AndroidTvAdbController> logger) : IDeviceController
{
    private readonly IAdbClient _adbClient = adbClient ?? throw new ArgumentNullException(nameof(adbClient));
    private readonly ILogger<AndroidTvAdbController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<bool> SendCommand(Zapper.Core.Models.Device device, DeviceCommand command)
    {
        if (!SupportsDevice(device))
        {
            _logger.LogWarning("Device {DeviceName} is not supported by ADB controller", device.Name);
            return false;
        }

        if (string.IsNullOrEmpty(device.IpAddress))
        {
            _logger.LogWarning("Device {DeviceName} has no IP address configured", device.Name);
            return false;
        }

        try
        {
            if (!_adbClient.IsConnected)
            {
                _logger.LogInformation("Connecting to Android TV device {DeviceName} at {IpAddress}", device.Name, device.IpAddress);
                var connected = await _adbClient.ConnectAsync(device.IpAddress, device.Port ?? 5555);
                if (!connected)
                {
                    _logger.LogError("Failed to connect to ADB on device {DeviceName}", device.Name);
                    return false;
                }
            }

            return command.Type switch
            {
                CommandType.Power => await HandlePowerCommand(command),
                CommandType.VolumeUp => await ExecuteKeyEvent(AndroidKeyEvents.VolumeUp),
                CommandType.VolumeDown => await ExecuteKeyEvent(AndroidKeyEvents.VolumeDown),
                CommandType.Mute => await ExecuteKeyEvent(AndroidKeyEvents.VolumeMute),
                CommandType.ChannelUp => await ExecuteKeyEvent(AndroidKeyEvents.ChannelUp),
                CommandType.ChannelDown => await ExecuteKeyEvent(AndroidKeyEvents.ChannelDown),
                CommandType.Input => await ExecuteKeyEvent(AndroidKeyEvents.Menu),
                CommandType.Menu => await ExecuteKeyEvent(AndroidKeyEvents.Menu),
                CommandType.Back => await ExecuteKeyEvent(AndroidKeyEvents.Back),
                CommandType.Home => await ExecuteKeyEvent(AndroidKeyEvents.Home),
                CommandType.Ok => await ExecuteKeyEvent(AndroidKeyEvents.DpadCenter),
                CommandType.DirectionalUp => await ExecuteKeyEvent(AndroidKeyEvents.DpadUp),
                CommandType.DirectionalDown => await ExecuteKeyEvent(AndroidKeyEvents.DpadDown),
                CommandType.DirectionalLeft => await ExecuteKeyEvent(AndroidKeyEvents.DpadLeft),
                CommandType.DirectionalRight => await ExecuteKeyEvent(AndroidKeyEvents.DpadRight),
                CommandType.Number => await HandleNumberCommand(command),
                CommandType.PlayPause => await ExecuteKeyEvent(AndroidKeyEvents.MediaPlayPause),
                CommandType.Stop => await ExecuteKeyEvent(AndroidKeyEvents.MediaStop),
                CommandType.FastForward => await ExecuteKeyEvent(AndroidKeyEvents.MediaFastForward),
                CommandType.Rewind => await ExecuteKeyEvent(AndroidKeyEvents.MediaRewind),
                CommandType.AppLaunch => await HandleAppLaunchCommand(command),
                CommandType.KeyboardInput => await HandleKeyboardInputCommand(command),
                CommandType.Custom => await HandleCustomCommand(command),
                _ => await HandleUnknownCommand(command)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send ADB command {CommandType} to device {DeviceName}",
                command.Type, device.Name);
            return false;
        }
    }

    public async Task<bool> TestConnection(Zapper.Core.Models.Device device)
    {
        if (!SupportsDevice(device))
            return false;

        if (string.IsNullOrEmpty(device.IpAddress))
            return false;

        try
        {
            var connected = await _adbClient.ConnectAsync(device.IpAddress, device.Port ?? 5555);
            if (connected)
            {
                var result = await _adbClient.ExecuteShellCommandWithResponseAsync("echo 'test'");
                return !string.IsNullOrEmpty(result);
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to test ADB connection to device {DeviceName}", device.Name);
            return false;
        }
    }

    public async Task<DeviceStatus> GetStatus(Zapper.Core.Models.Device device)
    {
        if (!SupportsDevice(device))
            return new DeviceStatus { IsOnline = false, StatusMessage = "Device not supported" };

        try
        {
            if (!_adbClient.IsConnected)
            {
                var connected = await _adbClient.ConnectAsync(device.IpAddress!, device.Port ?? 5555);
                if (!connected)
                    return new DeviceStatus { IsOnline = false, StatusMessage = "Failed to connect" };
            }

            var response = await _adbClient.ExecuteShellCommandWithResponseAsync("getprop ro.build.version.release");
            return !string.IsNullOrEmpty(response) 
                ? new DeviceStatus { IsOnline = true, StatusMessage = $"Android {response}" }
                : new DeviceStatus { IsOnline = false, StatusMessage = "No response from device" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get status for device {DeviceName}", device.Name);
            return new DeviceStatus { IsOnline = false, StatusMessage = ex.Message };
        }
    }

    public bool SupportsDevice(Zapper.Core.Models.Device device)
    {
        return device.ConnectionType == ConnectionType.Adb &&
               device.Type == DeviceType.AndroidTv &&
               !string.IsNullOrEmpty(device.IpAddress);
    }

    private async Task<bool> ExecuteKeyEvent(int keyCode)
    {
        var command = $"input keyevent {keyCode}";
        return await _adbClient.ExecuteShellCommandAsync(command);
    }

    private async Task<bool> HandlePowerCommand(DeviceCommand command)
    {
        return await ExecuteKeyEvent(AndroidKeyEvents.Power);
    }

    private async Task<bool> HandleNumberCommand(DeviceCommand command)
    {
        if (!string.IsNullOrEmpty(command.NetworkPayload) && int.TryParse(command.NetworkPayload, out var number))
        {
            var keyCode = number switch
            {
                0 => AndroidKeyEvents.Keycode0,
                1 => AndroidKeyEvents.Keycode1,
                2 => AndroidKeyEvents.Keycode2,
                3 => AndroidKeyEvents.Keycode3,
                4 => AndroidKeyEvents.Keycode4,
                5 => AndroidKeyEvents.Keycode5,
                6 => AndroidKeyEvents.Keycode6,
                7 => AndroidKeyEvents.Keycode7,
                8 => AndroidKeyEvents.Keycode8,
                9 => AndroidKeyEvents.Keycode9,
                _ => -1
            };

            if (keyCode != -1)
            {
                return await ExecuteKeyEvent(keyCode);
            }
        }

        _logger.LogWarning("Invalid number for number command: {Payload}", command.NetworkPayload);
        return false;
    }

    private async Task<bool> HandleAppLaunchCommand(DeviceCommand command)
    {
        if (string.IsNullOrEmpty(command.NetworkPayload))
        {
            _logger.LogWarning("App launch command has no package name");
            return false;
        }

        var packageName = command.NetworkPayload;

        var predefinedApps = new Dictionary<string, string>
        {
            { "netflix", "com.netflix.ninja" },
            { "youtube", "com.google.android.youtube.tv" },
            { "disney", "com.disney.disneyplus" },
            { "hulu", "com.hulu.plus" },
            { "primevideo", "com.amazon.avod.thirdpartyclient" },
            { "spotify", "com.spotify.tv.android" },
            { "plex", "com.plexapp.android" },
            { "kodi", "org.xbmc.kodi" }
        };

        if (predefinedApps.TryGetValue(packageName.ToLowerInvariant(), out var fullPackageName))
        {
            packageName = fullPackageName;
        }

        var launchCommand = $"am start -n {packageName}";
        var result = await _adbClient.ExecuteShellCommandAsync(launchCommand);

        if (!result)
        {
            var monkeyCommand = $"monkey -p {packageName} -c android.intent.category.LAUNCHER 1";
            result = await _adbClient.ExecuteShellCommandAsync(monkeyCommand);
        }

        return result;
    }

    private async Task<bool> HandleKeyboardInputCommand(DeviceCommand command)
    {
        if (string.IsNullOrEmpty(command.KeyboardText))
        {
            _logger.LogWarning("Keyboard input command has no text");
            return false;
        }

        var escapedText = command.KeyboardText.Replace(" ", "%s").Replace("'", "\\'");
        var inputCommand = $"input text '{escapedText}'";
        return await _adbClient.ExecuteShellCommandAsync(inputCommand);
    }

    private async Task<bool> HandleCustomCommand(DeviceCommand command)
    {
        if (string.IsNullOrEmpty(command.NetworkPayload))
        {
            _logger.LogWarning("Custom command has no payload");
            return false;
        }

        var payload = command.NetworkPayload;

        if (payload.StartsWith("shell:"))
        {
            var shellCommand = payload[6..];
            return await _adbClient.ExecuteShellCommandAsync(shellCommand);
        }

        if (payload.StartsWith("text:"))
        {
            var text = payload[5..];
            var escapedText = text.Replace(" ", "%s").Replace("'", "\\'");
            var inputCommand = $"input text '{escapedText}'";
            return await _adbClient.ExecuteShellCommandAsync(inputCommand);
        }

        if (payload.StartsWith("app:"))
        {
            var packageName = payload[4..];
            var launchCommand = $"am start -n {packageName}";
            return await _adbClient.ExecuteShellCommandAsync(launchCommand);
        }

        if (int.TryParse(payload, out var keyCode))
        {
            return await ExecuteKeyEvent(keyCode);
        }

        _logger.LogWarning("Unknown custom command payload: {Payload}", payload);
        return false;
    }

    private Task<bool> HandleUnknownCommand(DeviceCommand command)
    {
        _logger.LogWarning("Unknown ADB command type: {CommandType}", command.Type);
        return Task.FromResult(false);
    }
}