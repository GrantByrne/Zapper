using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Zapper.Device.Network;

namespace Zapper.Device.PlayStation;

public class PlayStationDeviceController(ILogger<PlayStationDeviceController> logger) : IPlayStationDeviceController
{
    private readonly ConcurrentDictionary<string, PlayStationConnection> _connections = new();
    private const int RemotePlayPort = 9295;

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
                logger.LogInformation("Already connected to PlayStation at {IpAddress}", device.IpAddress);
                return Task.FromResult(true);
            }

            var connection = new PlayStationConnection
            {
                IpAddress = device.IpAddress,
                LastActivity = DateTime.UtcNow
            };

            _connections.TryAdd(device.IpAddress, connection);
            logger.LogInformation("Connected to PlayStation at {IpAddress}", device.IpAddress);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to connect to PlayStation at {IpAddress}", device.IpAddress);
            return Task.FromResult(false);
        }
    }

    public Task<bool> DisconnectAsync(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(device.IpAddress))
            return Task.FromResult(false);

        _connections.TryRemove(device.IpAddress, out _);
        logger.LogInformation("Disconnected from PlayStation at {IpAddress}", device.IpAddress);
        return Task.FromResult(true);
    }

    public async Task<bool> SendCommandAsync(Zapper.Core.Models.Device device, Core.Models.DeviceCommand command, CancellationToken cancellationToken = default)
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
                Core.Models.CommandType.Power => await HandlePowerCommand(device, cancellationToken),
                Core.Models.CommandType.Menu => await SendButtonAsync(device.IpAddress, "ps", cancellationToken),
                Core.Models.CommandType.Back => await SendButtonAsync(device.IpAddress, "back", cancellationToken),
                Core.Models.CommandType.DirectionalUp => await SendButtonAsync(device.IpAddress, "up", cancellationToken),
                Core.Models.CommandType.DirectionalDown => await SendButtonAsync(device.IpAddress, "down", cancellationToken),
                Core.Models.CommandType.DirectionalLeft => await SendButtonAsync(device.IpAddress, "left", cancellationToken),
                Core.Models.CommandType.DirectionalRight => await SendButtonAsync(device.IpAddress, "right", cancellationToken),
                Core.Models.CommandType.Ok => await SendButtonAsync(device.IpAddress, "cross", cancellationToken),
                Core.Models.CommandType.PlayPause => await SendButtonAsync(device.IpAddress, "play_pause", cancellationToken),
                Core.Models.CommandType.Stop => await SendButtonAsync(device.IpAddress, "stop", cancellationToken),
                Core.Models.CommandType.FastForward => await SendButtonAsync(device.IpAddress, "r1", cancellationToken),
                Core.Models.CommandType.Rewind => await SendButtonAsync(device.IpAddress, "l1", cancellationToken),
                Core.Models.CommandType.Custom => await HandleCustomCommand(device.IpAddress, command, cancellationToken),
                _ => await HandleUnknownCommand(command, cancellationToken)
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send command {CommandType} to PlayStation device {DeviceName}", command.Type, device.Name);
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
            logger.LogError(ex, "Failed to test connection to PlayStation device {DeviceName}", device.Name);
            return false;
        }
    }

    public async Task<bool> PowerOnAsync(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(device.IpAddress))
        {
            logger.LogWarning("Device {DeviceName} missing IP for power on", device.Name);
            return false;
        }

        try
        {
            var payload = new
            {
                type = "power_on"
            };

            return await SendUdpCommandAsync(device.IpAddress, payload, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to power on PlayStation device {DeviceName}", device.Name);
            return false;
        }
    }

    public async Task<bool> PowerOffAsync(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(device.IpAddress))
            return false;

        return await SendButtonAsync(device.IpAddress, "power", cancellationToken);
    }

    public async Task<bool> NavigateAsync(Zapper.Core.Models.Device device, string direction, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(device.IpAddress))
            return false;

        return await SendButtonAsync(device.IpAddress, direction.ToLowerInvariant(), cancellationToken);
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
            await tcpClient.ConnectAsync(ipAddress, RemotePlayPort);

            var json = JsonSerializer.Serialize(payload);
            var data = Encoding.UTF8.GetBytes(json);

            await tcpClient.GetStream().WriteAsync(data, 0, data.Length, cancellationToken);

            logger.LogDebug("Sent TCP command to PlayStation at {IpAddress}: {Command}", ipAddress, json);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send TCP command to PlayStation at {IpAddress}", ipAddress);
            return false;
        }
    }

    private async Task<bool> SendUdpCommandAsync(string ipAddress, object payload, CancellationToken cancellationToken)
    {
        try
        {
            using var udpClient = new UdpClient();
            var endpoint = new IPEndPoint(IPAddress.Parse(ipAddress), RemotePlayPort);

            var json = JsonSerializer.Serialize(payload);
            var data = Encoding.UTF8.GetBytes(json);

            await udpClient.SendAsync(data, data.Length, endpoint);

            logger.LogDebug("Sent UDP command to PlayStation at {IpAddress}: {Command}", ipAddress, json);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send UDP command to PlayStation at {IpAddress}", ipAddress);
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

    private async Task<bool> HandleCustomCommand(string ipAddress, Core.Models.DeviceCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(command.NetworkPayload))
        {
            logger.LogWarning("Custom command has no payload");
            return false;
        }

        var payload = command.NetworkPayload.ToLowerInvariant();

        var buttonMap = new Dictionary<string, string>
        {
            ["cross"] = "cross",
            ["circle"] = "circle",
            ["square"] = "square",
            ["triangle"] = "triangle",
            ["l1"] = "l1",
            ["r1"] = "r1",
            ["l2"] = "l2",
            ["r2"] = "r2",
            ["share"] = "share",
            ["options"] = "options",
            ["ps"] = "ps",
            ["touchpad"] = "touchpad"
        };

        if (buttonMap.TryGetValue(payload, out var button))
        {
            return await SendButtonAsync(ipAddress, button, cancellationToken);
        }

        logger.LogWarning("Unknown custom command: {Payload}", command.NetworkPayload);
        return false;
    }

    private Task<bool> HandleUnknownCommand(Core.Models.DeviceCommand command, CancellationToken cancellationToken)
    {
        logger.LogWarning("Unknown PlayStation command type: {CommandType}", command.Type);
        return Task.FromResult(false);
    }

    private class PlayStationConnection
    {
        public string IpAddress { get; set; } = string.Empty;
        public DateTime LastActivity { get; set; }
    }
}