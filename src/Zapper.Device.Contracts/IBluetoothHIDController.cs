namespace Zapper.Device.Contracts;

public interface IBluetoothHIDController
{
    Task<bool> StartAdvertisingAsync(CancellationToken cancellationToken = default);
    Task<bool> StopAdvertisingAsync(CancellationToken cancellationToken = default);
    Task<bool> ConnectToDeviceAsync(string deviceId, CancellationToken cancellationToken = default);
    Task<bool> DisconnectAsync(CancellationToken cancellationToken = default);
    Task<bool> SendKeyEventAsync(HIDKeyCode keyCode, bool isPressed = true, CancellationToken cancellationToken = default);
    Task<bool> SendMouseEventAsync(int deltaX, int deltaY, bool leftClick = false, bool rightClick = false, CancellationToken cancellationToken = default);
    Task<bool> SendKeyboardTextAsync(string text, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetPairedDevicesAsync(CancellationToken cancellationToken = default);
    bool IsConnected { get; }
    bool IsAdvertising { get; }
    string? ConnectedDeviceId { get; }
    event EventHandler<string>? DeviceConnected;
    event EventHandler<string>? DeviceDisconnected;
}

public enum HIDKeyCode
{
    // Navigation keys
    DPadUp = 0x52,
    DPadDown = 0x51,
    DPadLeft = 0x50,
    DPadRight = 0x4F,
    DPadCenter = 0x28,
    
    // System keys
    Back = 0x29,
    Home = 0x4A,
    Menu = 0x76,
    Escape = 0x29,
    
    // Volume controls
    VolumeUp = 0x80,
    VolumeDown = 0x81,
    VolumeMute = 0x7F,
    
    // Media controls
    PlayPause = 0xCD,
    Play = 0xB0,
    Pause = 0xB1,
    Stop = 0xB7,
    FastForward = 0xB3,
    Rewind = 0xB4,
    NextTrack = 0xB5,
    PreviousTrack = 0xB6,
    
    // Function keys
    F1 = 0x3A,
    F2 = 0x3B,
    F3 = 0x3C,
    F4 = 0x3D,
    F5 = 0x3E,
    F6 = 0x3F,
    F7 = 0x40,
    F8 = 0x41,
    F9 = 0x42,
    F10 = 0x43,
    F11 = 0x44,
    F12 = 0x45,
    
    // Arrow keys
    ArrowUp = 0x52,
    ArrowDown = 0x51,
    ArrowLeft = 0x50,
    ArrowRight = 0x4F,
    
    // Common keys
    Enter = 0x28,
    Space = 0x2C,
    Tab = 0x2B,
    Backspace = 0x2A,
    Delete = 0x4C,
    
    // Numbers
    Number0 = 0x27,
    Number1 = 0x1E,
    Number2 = 0x1F,
    Number3 = 0x20,
    Number4 = 0x21,
    Number5 = 0x22,
    Number6 = 0x23,
    Number7 = 0x24,
    Number8 = 0x25,
    Number9 = 0x26
}