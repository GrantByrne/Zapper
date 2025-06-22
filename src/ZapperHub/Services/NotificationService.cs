using Microsoft.AspNetCore.SignalR;
using ZapperHub.Hubs;

namespace ZapperHub.Services;

public class NotificationService : INotificationService
{
    private readonly IHubContext<ZapperHubSignalR> _hubContext;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(IHubContext<ZapperHubSignalR> hubContext, ILogger<NotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyDeviceStatusChangedAsync(int deviceId, string deviceName, bool isOnline)
    {
        var data = new
        {
            DeviceId = deviceId,
            DeviceName = deviceName,
            IsOnline = isOnline,
            Timestamp = DateTime.UtcNow
        };

        await SendToDeviceGroupAsync(deviceId, "DeviceStatusChanged", data);
        await SendToAllClientsAsync("DeviceStatusChanged", data);
        
        _logger.LogInformation("Device status changed: {DeviceName} is now {Status}", 
            deviceName, isOnline ? "online" : "offline");
    }

    public async Task NotifyDeviceCommandExecutedAsync(int deviceId, string deviceName, string commandName, bool success)
    {
        var data = new
        {
            DeviceId = deviceId,
            DeviceName = deviceName,
            CommandName = commandName,
            Success = success,
            Timestamp = DateTime.UtcNow
        };

        await SendToDeviceGroupAsync(deviceId, "DeviceCommandExecuted", data);
        
        _logger.LogInformation("Device command executed: {DeviceName} - {CommandName} ({Status})", 
            deviceName, commandName, success ? "success" : "failed");
    }

    public async Task NotifyActivityStartedAsync(int activityId, string activityName)
    {
        var data = new
        {
            ActivityId = activityId,
            ActivityName = activityName,
            Status = "started",
            Timestamp = DateTime.UtcNow
        };

        await SendToActivityGroupAsync(activityId, "ActivityStatusChanged", data);
        await SendToAllClientsAsync("ActivityStatusChanged", data);
        
        _logger.LogInformation("Activity started: {ActivityName}", activityName);
    }

    public async Task NotifyActivityCompletedAsync(int activityId, string activityName, bool success)
    {
        var data = new
        {
            ActivityId = activityId,
            ActivityName = activityName,
            Status = success ? "completed" : "failed",
            Success = success,
            Timestamp = DateTime.UtcNow
        };

        await SendToActivityGroupAsync(activityId, "ActivityStatusChanged", data);
        await SendToAllClientsAsync("ActivityStatusChanged", data);
        
        _logger.LogInformation("Activity completed: {ActivityName} ({Status})", 
            activityName, success ? "success" : "failed");
    }

    public async Task NotifyActivityStepExecutedAsync(int activityId, string activityName, int stepNumber, string stepDescription, bool success)
    {
        var data = new
        {
            ActivityId = activityId,
            ActivityName = activityName,
            StepNumber = stepNumber,
            StepDescription = stepDescription,
            Success = success,
            Timestamp = DateTime.UtcNow
        };

        await SendToActivityGroupAsync(activityId, "ActivityStepExecuted", data);
        
        _logger.LogInformation("Activity step executed: {ActivityName} - Step {StepNumber} ({Status})", 
            activityName, stepNumber, success ? "success" : "failed");
    }

    public async Task NotifyDeviceDiscoveredAsync(string deviceType, string deviceName, string deviceAddress)
    {
        var data = new
        {
            DeviceType = deviceType,
            DeviceName = deviceName,
            DeviceAddress = deviceAddress,
            Timestamp = DateTime.UtcNow
        };

        await SendToAllClientsAsync("DeviceDiscovered", data);
        
        _logger.LogInformation("Device discovered: {DeviceName} ({DeviceType}) at {DeviceAddress}", 
            deviceName, deviceType, deviceAddress);
    }

    public async Task NotifyBluetoothDeviceConnectedAsync(string deviceId, string deviceName)
    {
        var data = new
        {
            DeviceId = deviceId,
            DeviceName = deviceName,
            Status = "connected",
            Timestamp = DateTime.UtcNow
        };

        await SendToAllClientsAsync("BluetoothDeviceStatusChanged", data);
        
        _logger.LogInformation("Bluetooth device connected: {DeviceName}", deviceName);
    }

    public async Task NotifyBluetoothDeviceDisconnectedAsync(string deviceId, string deviceName)
    {
        var data = new
        {
            DeviceId = deviceId,
            DeviceName = deviceName,
            Status = "disconnected",
            Timestamp = DateTime.UtcNow
        };

        await SendToAllClientsAsync("BluetoothDeviceStatusChanged", data);
        
        _logger.LogInformation("Bluetooth device disconnected: {DeviceName}", deviceName);
    }

    public async Task NotifyWebOSDevicePairedAsync(string deviceId, string deviceName, bool success)
    {
        var data = new
        {
            DeviceId = deviceId,
            DeviceName = deviceName,
            Success = success,
            Status = success ? "paired" : "pairing_failed",
            Timestamp = DateTime.UtcNow
        };

        await SendToAllClientsAsync("WebOSDevicePaired", data);
        
        _logger.LogInformation("WebOS device pairing: {DeviceName} ({Status})", 
            deviceName, success ? "success" : "failed");
    }

    public async Task SendSystemMessageAsync(string message, string level = "info")
    {
        var data = new
        {
            Message = message,
            Level = level,
            Timestamp = DateTime.UtcNow
        };

        await SendToAllClientsAsync("SystemMessage", data);
        
        _logger.LogInformation("System message sent: {Message} (level: {Level})", message, level);
    }

    public async Task SendToAllClientsAsync(string method, object data)
    {
        try
        {
            await _hubContext.Clients.Group("AllClients").SendAsync(method, data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message to all clients: {Method}", method);
        }
    }

    public async Task SendToDeviceGroupAsync(int deviceId, string method, object data)
    {
        try
        {
            await _hubContext.Clients.Group($"Device_{deviceId}").SendAsync(method, data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message to device group {DeviceId}: {Method}", deviceId, method);
        }
    }

    public async Task SendToActivityGroupAsync(int activityId, string method, object data)
    {
        try
        {
            await _hubContext.Clients.Group($"Activity_{activityId}").SendAsync(method, data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message to activity group {ActivityId}: {Method}", activityId, method);
        }
    }
}