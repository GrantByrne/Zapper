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
    IWebOSDeviceController webOSController,
    IRokuDeviceController rokuController,
    INotificationService notificationService,
    ILogger<DeviceService> logger) : IDeviceService
{

    public async Task<IEnumerable<Zapper.Core.Models.Device>> GetAllDevicesAsync()
    {
        return await context.Devices
            .Include(d => d.Commands)
            .OrderBy(d => d.Name)
            .ToListAsync();
    }

    public async Task<Zapper.Core.Models.Device?> GetDeviceAsync(int id)
    {
        return await context.Devices
            .Include(d => d.Commands)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<Zapper.Core.Models.Device> CreateDeviceAsync(Zapper.Core.Models.Device device)
    {
        device.CreatedAt = DateTime.UtcNow;
        device.LastSeen = DateTime.UtcNow;

        context.Devices.Add(device);
        await context.SaveChangesAsync();

        logger.LogInformation("Created device: {DeviceName} ({DeviceType})", device.Name, device.Type);
        return device;
    }

    public async Task<Zapper.Core.Models.Device?> UpdateDeviceAsync(int id, Zapper.Core.Models.Device device)
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

    public async Task<bool> DeleteDeviceAsync(int id)
    {
        var device = await context.Devices.FindAsync(id);
        if (device == null)
            return false;

        context.Devices.Remove(device);
        await context.SaveChangesAsync();

        logger.LogInformation("Deleted device: {DeviceName}", device.Name);
        return true;
    }

    public async Task<bool> SendCommandAsync(int deviceId, string commandName, CancellationToken cancellationToken = default)
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
            var success = await ExecuteCommandAsync(device, command, cancellationToken);
            
            if (success)
            {
                device.LastSeen = DateTime.UtcNow;
                device.IsOnline = true;
                await context.SaveChangesAsync();
                
                await notificationService.NotifyDeviceCommandExecutedAsync(device.Id, device.Name, commandName, true);
            }
            else
            {
                await notificationService.NotifyDeviceCommandExecutedAsync(device.Id, device.Name, commandName, false);
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

    public async Task<bool> TestDeviceConnectionAsync(int deviceId)
    {
        var device = await GetDeviceAsync(deviceId);
        if (device == null)
            return false;

        try
        {
            bool isOnline = device.ConnectionType switch
            {
                ConnectionType.NetworkTCP or ConnectionType.NetworkWebSocket => 
                    await TestNetworkDeviceAsync(device),
                ConnectionType.NetworkHTTP =>
                    await rokuController.TestConnectionAsync(device),
                ConnectionType.InfraredIR => 
                    irTransmitter.IsAvailable,
                ConnectionType.WebOS =>
                    await webOSController.TestConnectionAsync(device),
                _ => false
            };

            device.IsOnline = isOnline;
            device.LastSeen = DateTime.UtcNow;
            await context.SaveChangesAsync();
            
            await notificationService.NotifyDeviceStatusChangedAsync(device.Id, device.Name, isOnline);

            return isOnline;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to test connection for device {DeviceName}", device.Name);
            return false;
        }
    }

    public async Task<IEnumerable<Zapper.Core.Models.Device>> DiscoverDevicesAsync(string deviceType, CancellationToken cancellationToken = default)
    {
        try
        {
            var discoveryResult = await networkController.DiscoverDevicesAsync(deviceType, TimeSpan.FromSeconds(10), cancellationToken);
            
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

    private async Task<bool> ExecuteCommandAsync(Zapper.Core.Models.Device device, DeviceCommand command, CancellationToken cancellationToken)
    {
        logger.LogDebug("Executing command {CommandName} on device {DeviceName}", command.Name, device.Name);

        return device.ConnectionType switch
        {
            ConnectionType.InfraredIR => await ExecuteIrCommandAsync(command, cancellationToken),
            ConnectionType.NetworkTCP => await ExecuteNetworkCommandAsync(device, command, cancellationToken),
            ConnectionType.NetworkWebSocket => await ExecuteWebSocketCommandAsync(device, command, cancellationToken),
            ConnectionType.NetworkHTTP => await rokuController.SendCommandAsync(device, command, cancellationToken),
            ConnectionType.WebOS => await webOSController.SendCommandAsync(device, command, cancellationToken),
            _ => throw new NotSupportedException($"Connection type {device.ConnectionType} not supported")
        };
    }

    private async Task<bool> ExecuteIrCommandAsync(DeviceCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(command.IrCode))
        {
            logger.LogWarning("No IR code defined for command {CommandName}", command.Name);
            return false;
        }

        await irTransmitter.TransmitAsync(command.IrCode, command.IsRepeatable ? 3 : 1, cancellationToken);
        
        if (command.DelayMs > 0)
        {
            await Task.Delay(command.DelayMs, cancellationToken);
        }

        return true;
    }

    private async Task<bool> ExecuteNetworkCommandAsync(Zapper.Core.Models.Device device, DeviceCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(device.IpAddress) || !device.Port.HasValue)
        {
            logger.LogWarning("Missing IP address or port for network device {DeviceName}", device.Name);
            return false;
        }

        return await networkController.SendCommandAsync(
            device.IpAddress, 
            device.Port.Value, 
            command.Name, 
            command.NetworkPayload, 
            cancellationToken);
    }

    private async Task<bool> ExecuteWebSocketCommandAsync(Zapper.Core.Models.Device device, DeviceCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(device.IpAddress))
        {
            logger.LogWarning("Missing IP address for WebSocket device {DeviceName}", device.Name);
            return false;
        }

        var wsUrl = $"ws://{device.IpAddress}:{device.Port ?? 3000}";
        return await networkController.SendWebSocketCommandAsync(wsUrl, command.NetworkPayload ?? command.Name, cancellationToken);
    }

    private async Task<bool> TestNetworkDeviceAsync(Zapper.Core.Models.Device device)
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