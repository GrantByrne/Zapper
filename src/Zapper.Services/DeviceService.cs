using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Zapper.Data;
using Zapper.Integrations;
using Zapper.Core.Models;

namespace Zapper.Services;

public class DeviceService : IDeviceService
{
    private readonly ZapperContext _context;
    private readonly IInfraredTransmitter _irTransmitter;
    private readonly INetworkDeviceController _networkController;
    private readonly IWebOSDeviceController _webOSController;
    private readonly INotificationService _notificationService;
    private readonly ILogger<DeviceService> _logger;

    public DeviceService(
        ZapperContext context,
        IInfraredTransmitter irTransmitter,
        INetworkDeviceController networkController,
        IWebOSDeviceController webOSController,
        INotificationService notificationService,
        ILogger<DeviceService> logger)
    {
        _context = context;
        _irTransmitter = irTransmitter;
        _networkController = networkController;
        _webOSController = webOSController;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<IEnumerable<Device>> GetAllDevicesAsync()
    {
        return await _context.Devices
            .Include(d => d.Commands)
            .OrderBy(d => d.Name)
            .ToListAsync();
    }

    public async Task<Device?> GetDeviceAsync(int id)
    {
        return await _context.Devices
            .Include(d => d.Commands)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<Device> CreateDeviceAsync(Device device)
    {
        device.CreatedAt = DateTime.UtcNow;
        device.LastSeen = DateTime.UtcNow;

        _context.Devices.Add(device);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created device: {DeviceName} ({DeviceType})", device.Name, device.Type);
        return device;
    }

    public async Task<Device?> UpdateDeviceAsync(int id, Device device)
    {
        var existingDevice = await _context.Devices.FindAsync(id);
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

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated device: {DeviceName}", existingDevice.Name);
        return existingDevice;
    }

    public async Task<bool> DeleteDeviceAsync(int id)
    {
        var device = await _context.Devices.FindAsync(id);
        if (device == null)
            return false;

        _context.Devices.Remove(device);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted device: {DeviceName}", device.Name);
        return true;
    }

    public async Task<bool> SendCommandAsync(int deviceId, string commandName, CancellationToken cancellationToken = default)
    {
        var device = await _context.Devices
            .Include(d => d.Commands)
            .FirstOrDefaultAsync(d => d.Id == deviceId, cancellationToken);

        if (device == null)
        {
            _logger.LogWarning("Device not found: {DeviceId}", deviceId);
            return false;
        }

        var command = device.Commands.FirstOrDefault(c => c.Name.Equals(commandName, StringComparison.OrdinalIgnoreCase));
        if (command == null)
        {
            _logger.LogWarning("Command not found: {CommandName} for device {DeviceName}", commandName, device.Name);
            return false;
        }

        try
        {
            var success = await ExecuteCommandAsync(device, command, cancellationToken);
            
            if (success)
            {
                device.LastSeen = DateTime.UtcNow;
                device.IsOnline = true;
                await _context.SaveChangesAsync();
                
                // Notify clients of successful command execution
                await _notificationService.NotifyDeviceCommandExecutedAsync(device.Id, device.Name, commandName, true);
            }
            else
            {
                // Notify clients of failed command execution
                await _notificationService.NotifyDeviceCommandExecutedAsync(device.Id, device.Name, commandName, false);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send command {CommandName} to device {DeviceName}", commandName, device.Name);
            device.IsOnline = false;
            await _context.SaveChangesAsync();
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
                ConnectionType.InfraredIR => 
                    _irTransmitter.IsAvailable,
                ConnectionType.WebOS =>
                    await _webOSController.TestConnectionAsync(device),
                _ => false
            };

            device.IsOnline = isOnline;
            device.LastSeen = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            
            // Notify clients of device status change
            await _notificationService.NotifyDeviceStatusChangedAsync(device.Id, device.Name, isOnline);

            return isOnline;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to test connection for device {DeviceName}", device.Name);
            return false;
        }
    }

    public async Task<IEnumerable<Device>> DiscoverDevicesAsync(string deviceType, CancellationToken cancellationToken = default)
    {
        try
        {
            var discoveryResult = await _networkController.DiscoverDevicesAsync(deviceType, TimeSpan.FromSeconds(10), cancellationToken);
            
            if (string.IsNullOrEmpty(discoveryResult))
                return Enumerable.Empty<Device>();

            // Parse discovery results and create device objects
            // This is a simplified implementation
            var devices = new List<Device>();
            
            _logger.LogInformation("Device discovery completed for type: {DeviceType}", deviceType);
            return devices;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to discover devices of type {DeviceType}", deviceType);
            return Enumerable.Empty<Device>();
        }
    }

    private async Task<bool> ExecuteCommandAsync(Device device, DeviceCommand command, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Executing command {CommandName} on device {DeviceName}", command.Name, device.Name);

        return device.ConnectionType switch
        {
            ConnectionType.InfraredIR => await ExecuteIrCommandAsync(command, cancellationToken),
            ConnectionType.NetworkTCP => await ExecuteNetworkCommandAsync(device, command, cancellationToken),
            ConnectionType.NetworkWebSocket => await ExecuteWebSocketCommandAsync(device, command, cancellationToken),
            ConnectionType.WebOS => await _webOSController.SendCommandAsync(device, command, cancellationToken),
            _ => throw new NotSupportedException($"Connection type {device.ConnectionType} not supported")
        };
    }

    private async Task<bool> ExecuteIrCommandAsync(DeviceCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(command.IrCode))
        {
            _logger.LogWarning("No IR code defined for command {CommandName}", command.Name);
            return false;
        }

        await _irTransmitter.TransmitAsync(command.IrCode, command.IsRepeatable ? 3 : 1, cancellationToken);
        
        if (command.DelayMs > 0)
        {
            await Task.Delay(command.DelayMs, cancellationToken);
        }

        return true;
    }

    private async Task<bool> ExecuteNetworkCommandAsync(Device device, DeviceCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(device.IpAddress) || !device.Port.HasValue)
        {
            _logger.LogWarning("Missing IP address or port for network device {DeviceName}", device.Name);
            return false;
        }

        return await _networkController.SendCommandAsync(
            device.IpAddress, 
            device.Port.Value, 
            command.Name, 
            command.NetworkPayload, 
            cancellationToken);
    }

    private async Task<bool> ExecuteWebSocketCommandAsync(Device device, DeviceCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(device.IpAddress))
        {
            _logger.LogWarning("Missing IP address for WebSocket device {DeviceName}", device.Name);
            return false;
        }

        var wsUrl = $"ws://{device.IpAddress}:{device.Port ?? 3000}";
        return await _networkController.SendWebSocketCommandAsync(wsUrl, command.NetworkPayload ?? command.Name, cancellationToken);
    }

    private async Task<bool> TestNetworkDeviceAsync(Device device)
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