using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Zapper.Device.Network;

namespace Zapper.Device.Xbox;

public class XboxDeviceController(INetworkDeviceController networkController, ILogger<XboxDeviceController> logger) : IXboxDeviceController
{
    private readonly INetworkDeviceController _networkController = networkController;
    private readonly ConcurrentDictionary<string, XboxConnection> _connections = new();
    private const int CommandPort = 5050;

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
                logger.LogInformation("Already connected to Xbox at {IpAddress}", device.IpAddress);
                return Task.FromResult(true);
            }

            var connection = new XboxConnection
            {
                IpAddress = device.IpAddress,
                LiveId = device.AuthToken ?? string.Empty,
                LastActivity = DateTime.UtcNow
            };

            _connections.TryAdd(device.IpAddress, connection);
            logger.LogInformation("Connected to Xbox at {IpAddress}", device.IpAddress);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to connect to Xbox at {IpAddress}", device.IpAddress);
            return Task.FromResult(false);
        }
    }

    public Task<bool> DisconnectAsync(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(device.IpAddress))
            return Task.FromResult(false);

        _connections.TryRemove(device.IpAddress, out _);
        logger.LogInformation("Disconnected from Xbox at {IpAddress}", device.IpAddress);
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
                Zapper.Core.Models.CommandType.Menu => await SendButtonAsync(device.IpAddress, "nexus", cancellationToken),
                Zapper.Core.Models.CommandType.Back => await SendButtonAsync(device.IpAddress, "back", cancellationToken),
                Zapper.Core.Models.CommandType.DirectionalUp => await SendButtonAsync(device.IpAddress, "up", cancellationToken),
                Zapper.Core.Models.CommandType.DirectionalDown => await SendButtonAsync(device.IpAddress, "down", cancellationToken),
                Zapper.Core.Models.CommandType.DirectionalLeft => await SendButtonAsync(device.IpAddress, "left", cancellationToken),
                Zapper.Core.Models.CommandType.DirectionalRight => await SendButtonAsync(device.IpAddress, "right", cancellationToken),
                Zapper.Core.Models.CommandType.Ok => await SendButtonAsync(device.IpAddress, "a", cancellationToken),
                Zapper.Core.Models.CommandType.PlayPause => await SendButtonAsync(device.IpAddress, "play_pause", cancellationToken),
                Zapper.Core.Models.CommandType.Stop => await SendButtonAsync(device.IpAddress, "stop", cancellationToken),
                Zapper.Core.Models.CommandType.FastForward => await SendButtonAsync(device.IpAddress, "fast_forward", cancellationToken),
                Zapper.Core.Models.CommandType.Rewind => await SendButtonAsync(device.IpAddress, "rewind", cancellationToken),
                Zapper.Core.Models.CommandType.Number => await HandleNumberCommand(device.IpAddress, command, cancellationToken),
                Zapper.Core.Models.CommandType.Custom => await HandleCustomCommand(device.IpAddress, command, cancellationToken),
                _ => await HandleUnknownCommand(command, cancellationToken)
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send command {CommandType} to Xbox device {DeviceName}", command.Type, device.Name);
            return false;
        }
    }

    public async Task<bool> TestConnectionAsync(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(device.IpAddress))
            return false;

        try
        {
            return await SendPingAsync(device.IpAddress, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to test connection to Xbox device {DeviceName}", device.Name);
            return false;
        }
    }

    public async Task<bool> PowerOnAsync(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(device.IpAddress) || string.IsNullOrEmpty(device.AuthToken))
        {
            logger.LogWarning("Device {DeviceName} missing IP or Live ID for power on", device.Name);
            return false;
        }

        try
        {
            var payload = new
            {
                type = "power_on",
                live_id = device.AuthToken
            };

            return await SendUdpCommandAsync(device.IpAddress, payload, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to power on Xbox device {DeviceName}", device.Name);
            return false;
        }
    }

    public async Task<bool> PowerOffAsync(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(device.IpAddress))
            return false;

        return await SendButtonAsync(device.IpAddress, "power", cancellationToken);
    }

    public async Task<bool> SendTextAsync(Zapper.Core.Models.Device device, string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(device.IpAddress))
            return false;

        try
        {
            var payload = new
            {
                type = "text",
                text = text
            };

            return await SendTcpCommandAsync(device.IpAddress, payload, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send text to Xbox device {DeviceName}", device.Name);
            return false;
        }
    }

    private async Task<bool> SendButtonAsync(string ipAddress, string button, CancellationToken cancellationToken)
    {
        var payload = new
        {
            type = "button",
            button = button
        };

        return await SendTcpCommandAsync(ipAddress, payload, cancellationToken);
    }

    private async Task<bool> SendPingAsync(string ipAddress, CancellationToken cancellationToken)
    {
        var payload = new
        {
            type = "ping"
        };

        return await SendUdpCommandAsync(ipAddress, payload, cancellationToken);
    }

    private async Task<bool> SendTcpCommandAsync(string ipAddress, object payload, CancellationToken cancellationToken)
    {
        try
        {
            using var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(ipAddress, CommandPort);

            var json = JsonSerializer.Serialize(payload);
            var data = Encoding.UTF8.GetBytes(json);

            await tcpClient.GetStream().WriteAsync(data, 0, data.Length, cancellationToken);

            logger.LogDebug("Sent TCP command to Xbox at {IpAddress}: {Command}", ipAddress, json);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send TCP command to Xbox at {IpAddress}", ipAddress);
            return false;
        }
    }

    private async Task<bool> SendUdpCommandAsync(string ipAddress, object payload, CancellationToken cancellationToken)
    {
        try
        {
            using var udpClient = new UdpClient();
            var endpoint = new IPEndPoint(IPAddress.Parse(ipAddress), CommandPort);

            var json = JsonSerializer.Serialize(payload);
            var data = Encoding.UTF8.GetBytes(json);

            await udpClient.SendAsync(data, data.Length, endpoint);

            logger.LogDebug("Sent UDP command to Xbox at {IpAddress}: {Command}", ipAddress, json);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send UDP command to Xbox at {IpAddress}", ipAddress);
            return false;
        }
    }

    private async Task<bool> HandlePowerCommand(Zapper.Core.Models.Device device, CancellationToken cancellationToken)
    {
        var isPoweredOn = await TestConnectionAsync(device, cancellationToken);

        if (isPoweredOn)
        {
            return await PowerOffAsync(device, cancellationToken);
        }
        else
        {
            return await PowerOnAsync(device, cancellationToken);
        }
    }

    private async Task<bool> HandleNumberCommand(string ipAddress, Zapper.Core.Models.DeviceCommand command, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(command.NetworkPayload) && int.TryParse(command.NetworkPayload, out var number))
        {
            var keyCode = number switch
            {
                0 => "0",
                1 => "1",
                2 => "2",
                3 => "3",
                4 => "4",
                5 => "5",
                6 => "6",
                7 => "7",
                8 => "8",
                9 => "9",
                _ => null
            };

            if (!string.IsNullOrEmpty(keyCode))
            {
                return await SendButtonAsync(ipAddress, keyCode, cancellationToken);
            }
        }

        logger.LogWarning("Invalid number for number command: {Payload}", command.NetworkPayload);
        return false;
    }

    private async Task<bool> HandleCustomCommand(string ipAddress, Zapper.Core.Models.DeviceCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(command.NetworkPayload))
        {
            logger.LogWarning("Custom command has no payload");
            return false;
        }

        var payload = command.NetworkPayload.ToLowerInvariant();

        var buttonMap = new Dictionary<string, string>
        {
            ["a"] = "a",
            ["b"] = "b",
            ["x"] = "x",
            ["y"] = "y",
            ["lb"] = "left_shoulder",
            ["rb"] = "right_shoulder",
            ["lt"] = "left_trigger",
            ["rt"] = "right_trigger",
            ["view"] = "view",
            ["menu"] = "menu",
            ["xbox"] = "nexus",
            ["guide"] = "nexus",
            ["left_stick"] = "left_thumbstick",
            ["right_stick"] = "right_thumbstick"
        };

        if (buttonMap.TryGetValue(payload, out var button))
        {
            return await SendButtonAsync(ipAddress, button, cancellationToken);
        }

        logger.LogWarning("Unknown custom command: {Payload}", command.NetworkPayload);
        return false;
    }

    private Task<bool> HandleUnknownCommand(Zapper.Core.Models.DeviceCommand command, CancellationToken cancellationToken)
    {
        logger.LogWarning("Unknown Xbox command type: {CommandType}", command.Type);
        return Task.FromResult(false);
    }

    private class XboxConnection
    {
        public string IpAddress { get; set; } = string.Empty;
        public string LiveId { get; set; } = string.Empty;
        public DateTime LastActivity { get; set; }
    }
}