namespace Zapper.Device.USB;

public class RemoteButtonEventArgs : EventArgs
{
    public string DeviceId { get; }
    public string ButtonName { get; }
    public int KeyCode { get; }
    public DateTime Timestamp { get; }

    public RemoteButtonEventArgs(string deviceId, string buttonName, int keyCode)
    {
        DeviceId = deviceId;
        ButtonName = buttonName;
        KeyCode = keyCode;
        Timestamp = DateTime.UtcNow;
    }
}