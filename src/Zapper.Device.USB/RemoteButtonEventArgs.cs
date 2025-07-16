using Zapper.Core.Models;

namespace Zapper.Device.USB;

public class RemoteButtonEventArgs : EventArgs
{
    public string DeviceId { get; }
    public string ButtonName { get; }
    public int KeyCode { get; }
    public DateTime Timestamp { get; }
    public ButtonEventType EventType { get; }
    public TimeSpan? HoldDuration { get; }
    public byte[] RawData { get; }
    public bool IsRepeat { get; }
    public bool ShouldIntercept { get; set; }

    public RemoteButtonEventArgs(
        string deviceId,
        string buttonName,
        int keyCode,
        ButtonEventType eventType = ButtonEventType.KeyPress,
        TimeSpan? holdDuration = null,
        byte[]? rawData = null,
        bool isRepeat = false)
    {
        DeviceId = deviceId;
        ButtonName = buttonName;
        KeyCode = keyCode;
        Timestamp = DateTime.UtcNow;
        EventType = eventType;
        HoldDuration = holdDuration;
        RawData = rawData ?? Array.Empty<byte>();
        IsRepeat = isRepeat;
        ShouldIntercept = false;
    }
}