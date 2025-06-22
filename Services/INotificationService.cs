namespace ZapperHub.Services;

public interface INotificationService
{
    Task NotifyDeviceStatusChangedAsync(int deviceId, string deviceName, bool isOnline);
    Task NotifyDeviceCommandExecutedAsync(int deviceId, string deviceName, string commandName, bool success);
    Task NotifyActivityStartedAsync(int activityId, string activityName);
    Task NotifyActivityCompletedAsync(int activityId, string activityName, bool success);
    Task NotifyActivityStepExecutedAsync(int activityId, string activityName, int stepNumber, string stepDescription, bool success);
    Task NotifyDeviceDiscoveredAsync(string deviceType, string deviceName, string deviceAddress);
    Task NotifyBluetoothDeviceConnectedAsync(string deviceId, string deviceName);
    Task NotifyBluetoothDeviceDisconnectedAsync(string deviceId, string deviceName);
    Task NotifyWebOSDevicePairedAsync(string deviceId, string deviceName, bool success);
    Task SendSystemMessageAsync(string message, string level = "info");
    Task SendToAllClientsAsync(string method, object data);
    Task SendToDeviceGroupAsync(int deviceId, string method, object data);
    Task SendToActivityGroupAsync(int activityId, string method, object data);
}