using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Zapper.Data;
using Zapper.Device.Infrared;
using Zapper.Device.Network;
using Zapper.Device.WebOS;
using Zapper.Device.Roku;
using Zapper.Core.Models;

namespace Zapper.Services;

public class DeviceService(
    ZapperContext context,
    IInfraredTransmitter irTransmitter,
    INetworkDeviceController networkController,
    IWebOsDeviceController webOsController,
    IRokuDeviceController rokuController,
    INotificationService notificationService,
    IIrCodeService irCodeService,
    ILogger<DeviceService> logger) : IDeviceService
{

    public async Task<IEnumerable<Zapper.Core.Models.Device>> GetAllDevices()
    {
        return await context.Devices
            .Include(d => d.Commands)
            .OrderBy(d => d.Name)
            .ToListAsync();
    }

    public async Task<Zapper.Core.Models.Device?> GetDevice(int id)
    {
        return await context.Devices
            .Include(d => d.Commands)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<Zapper.Core.Models.Device> CreateDevice(Zapper.Core.Models.Device device)
    {
        device.CreatedAt = DateTime.UtcNow;
        device.LastSeen = DateTime.UtcNow;

        context.Devices.Add(device);
        await context.SaveChangesAsync();

        // If IR code set is specified, create commands from it
        if (device.ConnectionType == ConnectionType.InfraredIr && device.IrCodeSetId.HasValue)
        {
            var codeSet = await irCodeService.GetCodeSet(device.IrCodeSetId.Value);
            if (codeSet != null)
            {
                foreach (var irCode in codeSet.Codes)
                {
                    var command = new DeviceCommand
                    {
                        DeviceId = device.Id,
                        Name = irCode.CommandName,
                        Type = MapCommandType(irCode.CommandName),
                        IrCode = irCode.HexCode,
                        IsRepeatable = IsRepeatableCommand(irCode.CommandName)
                    };
                    context.DeviceCommands.Add(command);
                }
                await context.SaveChangesAsync();
                logger.LogInformation("Created {Count} IR commands for device {DeviceName} from code set {CodeSetId}",
                    codeSet.Codes.Count, device.Name, device.IrCodeSetId);
            }
        }

        logger.LogInformation("Created device: {DeviceName} ({DeviceType})", device.Name, device.Type);
        return device;
    }

    private static bool IsRepeatableCommand(string commandName)
    {
        var repeatableCommands = new[] { "VolumeUp", "VolumeDown", "ChannelUp", "ChannelDown",
            "DirectionalUp", "DirectionalDown", "DirectionalLeft", "DirectionalRight" };
        return repeatableCommands.Any(cmd => commandName.Contains(cmd, StringComparison.OrdinalIgnoreCase));
    }

    private static CommandType MapCommandType(string commandName)
    {
        return commandName.ToLowerInvariant() switch
        {
            "power" => CommandType.Power,
            "volumeup" => CommandType.VolumeUp,
            "volumedown" => CommandType.VolumeDown,
            "mute" => CommandType.Mute,
            "channelup" => CommandType.ChannelUp,
            "channeldown" => CommandType.ChannelDown,
            "input" => CommandType.Input,
            "menu" => CommandType.Menu,
            "back" => CommandType.Back,
            "home" => CommandType.Home,
            "ok" => CommandType.Ok,
            "directionalup" or "up" => CommandType.DirectionalUp,
            "directionaldown" or "down" => CommandType.DirectionalDown,
            "directionalleft" or "left" => CommandType.DirectionalLeft,
            "directionalright" or "right" => CommandType.DirectionalRight,
            "play" or "pause" or "playpause" => CommandType.PlayPause,
            "stop" => CommandType.Stop,
            "fastforward" or "forward" => CommandType.FastForward,
            "rewind" => CommandType.Rewind,
            "record" => CommandType.Record,
            _ => CommandType.Custom
        };
    }

    public async Task<Zapper.Core.Models.Device?> UpdateDevice(int id, Zapper.Core.Models.Device device)
    {
        var existingDevice = await context.Devices.FindAsync(id);
        if (existingDevice == null)
            return null;

        existingDevice.Name = device.Name;
        existingDevice.Brand = device.Brand;
        existingDevice.Model = device.Model;
        existingDevice.Type = device.Type;
        existingDevice.ConnectionType = device.ConnectionType;
        existingDevice.IpAddress = device.IpAddress;
        existingDevice.MacAddress = device.MacAddress;
        existingDevice.Port = device.Port;
        existingDevice.AuthToken = device.AuthToken;
        existingDevice.NetworkAddress = device.NetworkAddress;
        existingDevice.AuthenticationToken = device.AuthenticationToken;
        existingDevice.UseSecureConnection = device.UseSecureConnection;
        existingDevice.IrCodeSet = device.IrCodeSet;
        existingDevice.IsOnline = device.IsOnline;

        await context.SaveChangesAsync();

        logger.LogInformation("Updated device: {DeviceName}", existingDevice.Name);
        return existingDevice;
    }

    public async Task<bool> DeleteDevice(int id)
    {
        var device = await context.Devices.FindAsync(id);
        if (device == null)
            return false;

        context.Devices.Remove(device);
        await context.SaveChangesAsync();

        logger.LogInformation("Deleted device: {DeviceName}", device.Name);
        return true;
    }

    public async Task<bool> SendCommand(int deviceId, string commandName, CancellationToken cancellationToken = default)
    {
        var device = await context.Devices
            .Include(d => d.Commands)
            .FirstOrDefaultAsync(d => d.Id == deviceId, cancellationToken);

        if (device == null)
        {
            logger.LogWarning("Device not found: {DeviceId}", deviceId);
            return false;
        }

        var command = device.Commands.FirstOrDefault(c => c.Name.Equals(commandName, StringComparison.OrdinalIgnoreCase));
        if (command == null)
        {
            logger.LogWarning("Command not found: {CommandName} for device {DeviceName}", commandName, device.Name);
            return false;
        }

        try
        {
            var success = await ExecuteCommand(device, command, cancellationToken);

            if (success)
            {
                device.LastSeen = DateTime.UtcNow;
                device.IsOnline = true;
                await context.SaveChangesAsync();

                await notificationService.NotifyDeviceCommandExecuted(device.Id, device.Name, commandName, true);
            }
            else
            {
                await notificationService.NotifyDeviceCommandExecuted(device.Id, device.Name, commandName, false);
            }

            return success;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send command {CommandName} to device {DeviceName}", commandName, device.Name);
            device.IsOnline = false;
            await context.SaveChangesAsync();
            return false;
        }
    }

    public async Task<bool> SendCommand(int deviceId, Zapper.Contracts.Devices.SendCommandRequest request, CancellationToken cancellationToken = default)
    {
        var device = await context.Devices
            .Include(d => d.Commands)
            .FirstOrDefaultAsync(d => d.Id == deviceId, cancellationToken);

        if (device == null)
        {
            logger.LogWarning("Device not found: {DeviceId}", deviceId);
            return false;
        }

        DeviceCommand? command = null;

        if (request.Command.StartsWith("mouse_") || request.Command == "keyboard_input")
        {
            command = CreateVirtualCommand(request);
        }
        else if (request.Command.StartsWith("number_"))
        {
            var number = request.Command.Replace("number_", "");
            command = device.Commands.FirstOrDefault(c => c.Name.Equals($"Number{number}", StringComparison.OrdinalIgnoreCase))
                ?? device.Commands.FirstOrDefault(c => c.Name.Equals(number, StringComparison.OrdinalIgnoreCase))
                ?? CreateVirtualCommand(request);
        }
        else
        {
            command = device.Commands.FirstOrDefault(c => c.Name.Equals(request.Command, StringComparison.OrdinalIgnoreCase));
            if (command == null)
            {
                var mappedCommand = MapCommandName(request.Command);
                command = device.Commands.FirstOrDefault(c => c.Name.Equals(mappedCommand, StringComparison.OrdinalIgnoreCase));
            }
        }

        if (command == null)
        {
            logger.LogWarning("Command not found: {CommandName} for device {DeviceName}", request.Command, device.Name);
            return false;
        }

        if (request.MouseDeltaX.HasValue) command.MouseDeltaX = request.MouseDeltaX;
        if (request.MouseDeltaY.HasValue) command.MouseDeltaY = request.MouseDeltaY;
        if (!string.IsNullOrEmpty(request.KeyboardText)) command.NetworkPayload = request.KeyboardText;

        try
        {
            var success = await ExecuteCommand(device, command, cancellationToken);

            if (success)
            {
                device.LastSeen = DateTime.UtcNow;
                device.IsOnline = true;
                await context.SaveChangesAsync();

                await notificationService.NotifyDeviceCommandExecuted(device.Id, device.Name, request.Command, true);
            }
            else
            {
                await notificationService.NotifyDeviceCommandExecuted(device.Id, device.Name, request.Command, false);
            }

            return success;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send command {CommandName} to device {DeviceName}", request.Command, device.Name);
            device.IsOnline = false;
            await context.SaveChangesAsync();
            return false;
        }
    }

    private DeviceCommand CreateVirtualCommand(Zapper.Contracts.Devices.SendCommandRequest request)
    {
        return new DeviceCommand
        {
            Name = request.Command,
            Type = request.Command switch
            {
                "mouse_move" => CommandType.MouseMove,
                "mouse_click" => CommandType.MouseClick,
                "mouse_right_click" => CommandType.MouseClick,
                "keyboard_input" => CommandType.KeyboardInput,
                _ when request.Command.StartsWith("number_") => CommandType.Number,
                _ => CommandType.Custom
            },
            MouseDeltaX = request.MouseDeltaX,
            MouseDeltaY = request.MouseDeltaY,
            NetworkPayload = request.KeyboardText
        };
    }

    private string MapCommandName(string commandName)
    {
        return commandName.ToLowerInvariant() switch
        {
            "power" => "Power",
            "up" => "DirectionalUp",
            "down" => "DirectionalDown",
            "left" => "DirectionalLeft",
            "right" => "DirectionalRight",
            "ok" => "Ok",
            "volume_up" => "VolumeUp",
            "volume_down" => "VolumeDown",
            "mute" => "Mute",
            _ => commandName
        };
    }

    public async Task<bool> TestDeviceConnection(int deviceId)
    {
        var device = await GetDevice(deviceId);
        if (device == null)
            return false;

        try
        {
            bool isOnline = device.ConnectionType switch
            {
                ConnectionType.NetworkTcp or ConnectionType.NetworkWebSocket =>
                    await TestNetworkDevice(device),
                ConnectionType.NetworkHttp =>
                    await rokuController.TestConnection(device),
                ConnectionType.InfraredIr =>
                    irTransmitter.IsAvailable,
                ConnectionType.WebOs =>
                    await webOsController.TestConnection(device),
                _ => false
            };

            device.IsOnline = isOnline;
            device.LastSeen = DateTime.UtcNow;
            await context.SaveChangesAsync();

            await notificationService.NotifyDeviceStatusChanged(device.Id, device.Name, isOnline);

            return isOnline;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to test connection for device {DeviceName}", device.Name);
            return false;
        }
    }

    public async Task<IEnumerable<Zapper.Core.Models.Device>> DiscoverDevices(string deviceType, CancellationToken cancellationToken = default)
    {
        try
        {
            var discoveryResult = await networkController.DiscoverDevices(deviceType, TimeSpan.FromSeconds(10), cancellationToken);

            if (string.IsNullOrEmpty(discoveryResult))
                return [];

            var devices = new List<Zapper.Core.Models.Device>();

            logger.LogInformation("Device discovery completed for type: {DeviceType}", deviceType);
            return devices;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to discover devices of type {DeviceType}", deviceType);
            return [];
        }
    }

    private async Task<bool> ExecuteCommand(Zapper.Core.Models.Device device, DeviceCommand command, CancellationToken cancellationToken)
    {
        logger.LogDebug("Executing command {CommandName} on device {DeviceName}", command.Name, device.Name);

        return device.ConnectionType switch
        {
            ConnectionType.InfraredIr => await ExecuteIrCommand(command, cancellationToken),
            ConnectionType.NetworkTcp => await ExecuteNetworkCommand(device, command, cancellationToken),
            ConnectionType.NetworkWebSocket => await ExecuteWebSocketCommand(device, command, cancellationToken),
            ConnectionType.NetworkHttp => await rokuController.SendCommand(device, command, cancellationToken),
            ConnectionType.WebOs => await webOsController.SendCommand(device, command, cancellationToken),
            _ => throw new NotSupportedException($"Connection type {device.ConnectionType} not supported")
        };
    }

    private async Task<bool> ExecuteIrCommand(DeviceCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(command.IrCode))
        {
            logger.LogWarning("No IR code defined for command {CommandName}", command.Name);
            return false;
        }

        await irTransmitter.Transmit(command.IrCode, command.IsRepeatable ? 3 : 1, cancellationToken);

        if (command.DelayMs > 0)
        {
            await Task.Delay(command.DelayMs, cancellationToken);
        }

        return true;
    }

    private async Task<bool> ExecuteNetworkCommand(Zapper.Core.Models.Device device, DeviceCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(device.IpAddress) || !device.Port.HasValue)
        {
            logger.LogWarning("Missing IP address or port for network device {DeviceName}", device.Name);
            return false;
        }

        return await networkController.SendCommand(
            device.IpAddress,
            device.Port.Value,
            command.Name,
            command.NetworkPayload,
            cancellationToken);
    }

    private async Task<bool> ExecuteWebSocketCommand(Zapper.Core.Models.Device device, DeviceCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(device.IpAddress))
        {
            logger.LogWarning("Missing IP address for WebSocket device {DeviceName}", device.Name);
            return false;
        }

        var wsUrl = $"ws://{device.IpAddress}:{device.Port ?? 3000}";
        return await networkController.SendWebSocketCommand(wsUrl, command.NetworkPayload ?? command.Name, cancellationToken);
    }

    private async Task<bool> TestNetworkDevice(Zapper.Core.Models.Device device)
    {
        if (string.IsNullOrEmpty(device.IpAddress))
            return false;

        try
        {
            using var ping = new System.Net.NetworkInformation.Ping();
            var reply = await ping.SendPingAsync(device.IpAddress, 5000);
            return reply.Status == System.Net.NetworkInformation.IPStatus.Success;
        }
        catch
        {
            return false;
        }
    }
}