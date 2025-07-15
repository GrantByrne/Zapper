using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Zapper.Services;

public class NotificationService(IHubContext<ZapperSignalR> hubContext, ILogger<NotificationService> logger) : INotificationService
{

    public async Task NotifyDeviceStatusChanged(int deviceId, string deviceName, bool isOnline)
    {
        var data = new
        {
            DeviceId = deviceId,
            DeviceName = deviceName,
            IsOnline = isOnline,
            Timestamp = DateTime.UtcNow
        };

        await SendToDeviceGroup(deviceId, "DeviceStatusChanged", data);
        await SendToAllClients("DeviceStatusChanged", data);

        logger.LogInformation("Device status changed: {DeviceName} is now {Status}",
            deviceName, isOnline ? "online" : "offline");
    }

    public async Task NotifyDeviceCommandExecuted(int deviceId, string deviceName, string commandName, bool success)
    {
        var data = new
        {
            DeviceId = deviceId,
            DeviceName = deviceName,
            CommandName = commandName,
            Success = success,
            Timestamp = DateTime.UtcNow
        };

        await SendToDeviceGroup(deviceId, "DeviceCommandExecuted", data);

        logger.LogInformation("Device command executed: {DeviceName} - {CommandName} ({Status})",
            deviceName, commandName, success ? "success" : "failed");
    }

    public async Task NotifyActivityStarted(int activityId, string activityName)
    {
        var data = new
        {
            ActivityId = activityId,
            ActivityName = activityName,
            Status = "started",
            Timestamp = DateTime.UtcNow
        };

        await SendToActivityGroup(activityId, "ActivityStatusChanged", data);
        await SendToAllClients("ActivityStatusChanged", data);

        logger.LogInformation("Activity started: {ActivityName}", activityName);
    }

    public async Task NotifyActivityCompleted(int activityId, string activityName, bool success)
    {
        var data = new
        {
            ActivityId = activityId,
            ActivityName = activityName,
            Status = success ? "completed" : "failed",
            Success = success,
            Timestamp = DateTime.UtcNow
        };

        await SendToActivityGroup(activityId, "ActivityStatusChanged", data);
        await SendToAllClients("ActivityStatusChanged", data);

        logger.LogInformation("Activity completed: {ActivityName} ({Status})",
            activityName, success ? "success" : "failed");
    }

    public async Task NotifyActivityStepExecuted(int activityId, string activityName, int stepNumber, string stepDescription, bool success)
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

        await SendToActivityGroup(activityId, "ActivityStepExecuted", data);

        logger.LogInformation("Activity step executed: {ActivityName} - Step {StepNumber} ({Status})",
            activityName, stepNumber, success ? "success" : "failed");
    }

    public async Task NotifyDeviceDiscovered(string deviceType, string deviceName, string deviceAddress)
    {
        var data = new
        {
            DeviceType = deviceType,
            DeviceName = deviceName,
            DeviceAddress = deviceAddress,
            Timestamp = DateTime.UtcNow
        };

        await SendToAllClients("DeviceDiscovered", data);

        logger.LogInformation("Device discovered: {DeviceName} ({DeviceType}) at {DeviceAddress}",
            deviceName, deviceType, deviceAddress);
    }

    public async Task NotifyBluetoothDeviceConnected(string deviceId, string deviceName)
    {
        var data = new
        {
            DeviceId = deviceId,
            DeviceName = deviceName,
            Status = "connected",
            Timestamp = DateTime.UtcNow
        };

        await SendToAllClients("BluetoothDeviceStatusChanged", data);

        logger.LogInformation("Bluetooth device connected: {DeviceName}", deviceName);
    }

    public async Task NotifyBluetoothDeviceDisconnected(string deviceId, string deviceName)
    {
        var data = new
        {
            DeviceId = deviceId,
            DeviceName = deviceName,
            Status = "disconnected",
            Timestamp = DateTime.UtcNow
        };

        await SendToAllClients("BluetoothDeviceStatusChanged", data);

        logger.LogInformation("Bluetooth device disconnected: {DeviceName}", deviceName);
    }

    public async Task NotifyWebOsDevicePaired(string deviceId, string deviceName, bool success)
    {
        var data = new
        {
            DeviceId = deviceId,
            DeviceName = deviceName,
            Success = success,
            Status = success ? "paired" : "pairing_failed",
            Timestamp = DateTime.UtcNow
        };

        await SendToAllClients("WebOSDevicePaired", data);

        logger.LogInformation("WebOS device pairing: {DeviceName} ({Status})",
            deviceName, success ? "success" : "failed");
    }

    public async Task SendSystemMessage(string message, string level = "info")
    {
        var data = new
        {
            Message = message,
            Level = level,
            Timestamp = DateTime.UtcNow
        };

        await SendToAllClients("SystemMessage", data);

        logger.LogInformation("System message sent: {Message} (level: {Level})", message, level);
    }

    public async Task SendToAllClients(string method, object data)
    {
        try
        {
            await hubContext.Clients.Group("AllClients").SendAsync(method, data);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send message to all clients: {Method}", method);
        }
    }

    public async Task SendToDeviceGroup(int deviceId, string method, object data)
    {
        try
        {
            await hubContext.Clients.Group($"Device_{deviceId}").SendAsync(method, data);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send message to device group {DeviceId}: {Method}", deviceId, method);
        }
    }

    public async Task SendToActivityGroup(int activityId, string method, object data)
    {
        try
        {
            await hubContext.Clients.Group($"Activity_{activityId}").SendAsync(method, data);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send message to activity group {ActivityId}: {Method}", activityId, method);
        }
    }
}