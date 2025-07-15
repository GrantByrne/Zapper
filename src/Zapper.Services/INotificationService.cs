namespace Zapper.Services;

public interface INotificationService
{
    Task NotifyDeviceStatusChanged(int deviceId, string deviceName, bool isOnline);
    Task NotifyDeviceCommandExecuted(int deviceId, string deviceName, string commandName, bool success);
    Task NotifyActivityStarted(int activityId, string activityName);
    Task NotifyActivityCompleted(int activityId, string activityName, bool success);
    Task NotifyActivityStepExecuted(int activityId, string activityName, int stepNumber, string stepDescription, bool success);
    Task NotifyDeviceDiscovered(string deviceType, string deviceName, string deviceAddress);
    Task NotifyBluetoothDeviceConnected(string deviceId, string deviceName);
    Task NotifyBluetoothDeviceDisconnected(string deviceId, string deviceName);
    Task NotifyWebOsDevicePaired(string deviceId, string deviceName, bool success);
    Task SendSystemMessage(string message, string level = "info");
    Task SendToAllClients(string method, object data);
    Task SendToDeviceGroup(int deviceId, string method, object data);
    Task SendToActivityGroup(int activityId, string method, object data);
}