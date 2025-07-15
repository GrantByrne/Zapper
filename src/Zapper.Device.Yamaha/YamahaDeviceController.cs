using System.Collections.Concurrent;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Zapper.Device.Yamaha;

public class YamahaDeviceController(HttpClient httpClient, ILogger<YamahaDeviceController> logger) : IYamahaDeviceController
{
    private readonly ConcurrentDictionary<string, YamahaConnection> _connections = new();
    private const int MusicCastPort = 80;

    public Task<bool> ConnectAsync(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(device.IpAddress))
        {
            logger.LogWarning("Device {DeviceName} has no IP address configured", device.Name);
            return Task.FromResult(false);
        }

        try
        {
            if (_connections.ContainsKey(device.IpAddress))
            {
                logger.LogInformation("Already connected to Yamaha at {IpAddress}", device.IpAddress);
                return Task.FromResult(true);
            }

            var connection = new YamahaConnection
            {
                IpAddress = device.IpAddress,
                LastActivity = DateTime.UtcNow,
                ProtocolType = YamahaProtocolType.MusicCast
            };

            _connections.TryAdd(device.IpAddress, connection);
            logger.LogInformation("Connected to Yamaha at {IpAddress}", device.IpAddress);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to connect to Yamaha at {IpAddress}", device.IpAddress);
            return Task.FromResult(false);
        }
    }

    public Task<bool> DisconnectAsync(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(device.IpAddress))
            return Task.FromResult(false);

        _connections.TryRemove(device.IpAddress, out _);
        logger.LogInformation("Disconnected from Yamaha at {IpAddress}", device.IpAddress);
        return Task.FromResult(true);
    }

    public async Task<bool> SendCommandAsync(Zapper.Core.Models.Device device, Zapper.Core.Models.DeviceCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(device.IpAddress))
        {
            logger.LogWarning("Device {DeviceName} has no IP address configured", device.Name);
            return false;
        }

        try
        {
            return command.Type switch
            {
                Zapper.Core.Models.CommandType.Power => await HandlePowerCommand(device, cancellationToken),
                Zapper.Core.Models.CommandType.VolumeUp => await AdjustVolumeAsync(device, 5, cancellationToken),
                Zapper.Core.Models.CommandType.VolumeDown => await AdjustVolumeAsync(device, -5, cancellationToken),
                Zapper.Core.Models.CommandType.Mute => await MuteAsync(device, cancellationToken),
                Zapper.Core.Models.CommandType.Input => await HandleInputCommand(device, command, cancellationToken),
                Zapper.Core.Models.CommandType.Custom => await HandleCustomCommand(device, command, cancellationToken),
                _ => await HandleUnknownCommand(command, cancellationToken)
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send command {CommandType} to Yamaha device {DeviceName}", command.Type, device.Name);
            return false;
        }
    }

    public async Task<bool> TestConnectionAsync(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(device.IpAddress))
            return false;

        try
        {
            var url = $"http://{device.IpAddress}/YamahaExtendedControl/v1/system/getFeatures";
            var response = await httpClient.GetAsync(url, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to test connection to Yamaha device {DeviceName}", device.Name);
            return false;
        }
    }

    public async Task<bool> PowerOnAsync(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(device.IpAddress))
            return false;

        try
        {
            var url = $"http://{device.IpAddress}/YamahaExtendedControl/v1/main/setPower?power=on";
            var response = await httpClient.GetAsync(url, cancellationToken);
            logger.LogDebug("Sent power on command to Yamaha at {IpAddress}", device.IpAddress);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to power on Yamaha device {DeviceName}", device.Name);
            return false;
        }
    }

    public async Task<bool> PowerOffAsync(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(device.IpAddress))
            return false;

        try
        {
            var url = $"http://{device.IpAddress}/YamahaExtendedControl/v1/main/setPower?power=standby";
            var response = await httpClient.GetAsync(url, cancellationToken);
            logger.LogDebug("Sent power off command to Yamaha at {IpAddress}", device.IpAddress);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to power off Yamaha device {DeviceName}", device.Name);
            return false;
        }
    }

    public async Task<bool> SetVolumeAsync(Zapper.Core.Models.Device device, int volume, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(device.IpAddress))
            return false;

        try
        {
            var clampedVolume = Math.Clamp(volume, 0, 100);
            var url = $"http://{device.IpAddress}/YamahaExtendedControl/v1/main/setVolume?volume={clampedVolume}";
            var response = await httpClient.GetAsync(url, cancellationToken);
            logger.LogDebug("Set volume to {Volume} on Yamaha at {IpAddress}", clampedVolume, device.IpAddress);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to set volume on Yamaha device {DeviceName}", device.Name);
            return false;
        }
    }

    public async Task<bool> SetInputAsync(Zapper.Core.Models.Device device, string input, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(device.IpAddress))
            return false;

        try
        {
            var url = $"http://{device.IpAddress}/YamahaExtendedControl/v1/main/setInput?input={input}";
            var response = await httpClient.GetAsync(url, cancellationToken);
            logger.LogDebug("Set input to {Input} on Yamaha at {IpAddress}", input, device.IpAddress);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to set input on Yamaha device {DeviceName}", device.Name);
            return false;
        }
    }

    public async Task<bool> MuteAsync(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(device.IpAddress))
            return false;

        try
        {
            var isMuted = await GetMuteStatusAsync(device, cancellationToken);
            var newMuteState = !isMuted;
            var url = $"http://{device.IpAddress}/YamahaExtendedControl/v1/main/setMute?enable={newMuteState.ToString().ToLowerInvariant()}";
            var response = await httpClient.GetAsync(url, cancellationToken);
            logger.LogDebug("Set mute to {Mute} on Yamaha at {IpAddress}", newMuteState, device.IpAddress);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to toggle mute on Yamaha device {DeviceName}", device.Name);
            return false;
        }
    }

    private async Task<bool> HandlePowerCommand(Zapper.Core.Models.Device device, CancellationToken cancellationToken)
    {
        var isPoweredOn = await GetPowerStatusAsync(device, cancellationToken);

        if (isPoweredOn)
        {
            return await PowerOffAsync(device, cancellationToken);
        }
        else
        {
            return await PowerOnAsync(device, cancellationToken);
        }
    }

    private async Task<bool> HandleInputCommand(Zapper.Core.Models.Device device, Zapper.Core.Models.DeviceCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(command.NetworkPayload))
        {
            logger.LogWarning("Input command has no payload");
            return false;
        }

        return await SetInputAsync(device, command.NetworkPayload, cancellationToken);
    }

    private async Task<bool> HandleCustomCommand(Zapper.Core.Models.Device device, Zapper.Core.Models.DeviceCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(command.NetworkPayload))
        {
            logger.LogWarning("Custom command has no payload");
            return false;
        }

        var payload = command.NetworkPayload.ToLowerInvariant();

        var inputMap = new Dictionary<string, string>
        {
            ["hdmi1"] = "hdmi1",
            ["hdmi2"] = "hdmi2",
            ["hdmi3"] = "hdmi3",
            ["hdmi4"] = "hdmi4",
            ["optical"] = "optical1",
            ["coax"] = "coax1",
            ["aux"] = "aux",
            ["bluetooth"] = "bluetooth",
            ["wifi"] = "server",
            ["usb"] = "usb",
            ["fm"] = "tuner",
            ["am"] = "tuner"
        };

        if (inputMap.TryGetValue(payload, out var input))
        {
            return await SetInputAsync(device, input, cancellationToken);
        }

        logger.LogWarning("Unknown custom command: {Payload}", command.NetworkPayload);
        return false;
    }

    private async Task<bool> GetPowerStatusAsync(Zapper.Core.Models.Device device, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(device.IpAddress))
            return false;

        try
        {
            var url = $"http://{device.IpAddress}/YamahaExtendedControl/v1/main/getStatus";
            var response = await httpClient.GetAsync(url, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var statusResponse = JsonSerializer.Deserialize<YamahaStatusResponse>(content, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });
                return statusResponse?.Power == "on";
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get power status from Yamaha device {DeviceName}", device.Name);
        }

        return false;
    }

    private async Task<bool> GetMuteStatusAsync(Zapper.Core.Models.Device device, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(device.IpAddress))
            return false;

        try
        {
            var url = $"http://{device.IpAddress}/YamahaExtendedControl/v1/main/getStatus";
            var response = await httpClient.GetAsync(url, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var statusResponse = JsonSerializer.Deserialize<YamahaStatusResponse>(content, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });
                return statusResponse?.Mute ?? false;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get mute status from Yamaha device {DeviceName}", device.Name);
        }

        return false;
    }

    private async Task<bool> AdjustVolumeAsync(Zapper.Core.Models.Device device, int adjustment, CancellationToken cancellationToken)
    {
        var currentVolume = await GetCurrentVolumeAsync(device, cancellationToken);
        var newVolume = Math.Clamp(currentVolume + adjustment, 0, 100);
        return await SetVolumeAsync(device, newVolume, cancellationToken);
    }

    private async Task<int> GetCurrentVolumeAsync(Zapper.Core.Models.Device device, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(device.IpAddress))
            return 0;

        try
        {
            var url = $"http://{device.IpAddress}/YamahaExtendedControl/v1/main/getStatus";
            var response = await httpClient.GetAsync(url, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var statusResponse = JsonSerializer.Deserialize<YamahaStatusResponse>(content, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });
                return statusResponse?.Volume ?? 50;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get current volume from Yamaha device {DeviceName}", device.Name);
        }

        return 50;
    }

    private Task<bool> HandleUnknownCommand(Zapper.Core.Models.DeviceCommand command, CancellationToken cancellationToken)
    {
        logger.LogWarning("Unknown Yamaha command type: {CommandType}", command.Type);
        return Task.FromResult(false);
    }

    private class YamahaConnection
    {
        public string IpAddress { get; set; } = string.Empty;
        public DateTime LastActivity { get; set; }
        public YamahaProtocolType ProtocolType { get; set; }
    }

    private enum YamahaProtocolType
    {
        MusicCast,
        Ynca
    }

    private class YamahaStatusResponse
    {
        public string Power { get; set; } = string.Empty;
        public int Volume { get; set; }
        public bool Mute { get; set; }
        public string Input { get; set; } = string.Empty;
    }
}