namespace Zapper.Device.Bluetooth;

public interface IBluetoothHidController
{
    Task<bool> SendKeyAsync(string deviceAddress, HidKeyCode keyCode, CancellationToken cancellationToken = default);
    Task<bool> SendKeySequenceAsync(string deviceAddress, HidKeyCode[] keyCodes, int delayMs = 50, CancellationToken cancellationToken = default);
    Task<bool> SendTextAsync(string deviceAddress, string text, CancellationToken cancellationToken = default);
    Task<bool> ConnectAsync(string deviceAddress, CancellationToken cancellationToken = default);
    Task<bool> DisconnectAsync(string deviceAddress, CancellationToken cancellationToken = default);
    Task<bool> IsConnectedAsync(string deviceAddress, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetConnectedDevicesAsync(CancellationToken cancellationToken = default);
}

public enum HidKeyCode
{
    None = 0x00,

    A = 0x04,
    B = 0x05,
    C = 0x06,
    D = 0x07,
    E = 0x08,
    F = 0x09,
    G = 0x0A,
    H = 0x0B,
    I = 0x0C,
    J = 0x0D,
    K = 0x0E,
    L = 0x0F,
    M = 0x10,
    N = 0x11,
    O = 0x12,
    P = 0x13,
    Q = 0x14,
    R = 0x15,
    S = 0x16,
    T = 0x17,
    U = 0x18,
    V = 0x19,
    W = 0x1A,
    X = 0x1B,
    Y = 0x1C,
    Z = 0x1D,

    Key1 = 0x1E,
    Key2 = 0x1F,
    Key3 = 0x20,
    Key4 = 0x21,
    Key5 = 0x22,
    Key6 = 0x23,
    Key7 = 0x24,
    Key8 = 0x25,
    Key9 = 0x26,
    Key0 = 0x27,

    Enter = 0x28,
    Escape = 0x29,
    Backspace = 0x2A,
    Tab = 0x2B,
    Space = 0x2C,

    DPadUp = 0x52,
    DPadDown = 0x51,
    DPadLeft = 0x50,
    DPadRight = 0x4F,
    DPadCenter = 0x28,

    ArrowUp = DPadUp,
    ArrowDown = DPadDown,
    ArrowLeft = DPadLeft,
    ArrowRight = DPadRight,

    PageUp = 0x4B,
    PageDown = 0x4E,

    Back = 0x29,
    Home = 0x4A,
    Menu = 0x76,
    Delete = 0x4C,

    VolumeUp = 0x80,
    VolumeDown = 0x81,
    VolumeMute = 0x7F,

    PlayPause = 0xCD,
    Play = 0xB0,
    Pause = 0xB1,
    Stop = 0xB7,
    FastForward = 0xB3,
    Rewind = 0xB4,
    NextTrack = 0xB5,
    PreviousTrack = 0xB6,

    Assistant = 0xAE,
    Search = 0x221,
    Settings = 0x222,

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
    F12 = 0x45
}