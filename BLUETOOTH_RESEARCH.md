# Bluetooth Remote Control Support Research

## Overview
Research findings for implementing Bluetooth HID (Human Interface Device) support for Android TV and Apple TV remote control functionality in Zapper.

## Android TV Bluetooth HID Implementation

### Protocol Details
- **Technology**: Bluetooth HID (Human Interface Device) Profile
- **Connection**: Direct Bluetooth pairing and connection
- **Communication**: HID reports for key events and mouse/touchpad input
- **Port Usage**: TCP ports 6466/6467 for network-based alternatives

### HID Descriptor Requirements
```
Consumer Device Page (0x0C) for:
- Volume controls (up/down/mute)
- Directional navigation (up/down/left/right/center)
- Media controls (play/pause/stop/ff/rewind)
- Home/back/menu buttons

Generic Desktop Page (0x01) for:
- Mouse movement and clicks
- Keyboard input simulation
```

### Key Implementation Components
1. **Bluetooth HID Device Simulation**
   - Act as Bluetooth peripheral device
   - Implement HID keyboard/mouse descriptors
   - Handle pairing and connection management

2. **Supported Controls**
   - D-pad navigation (up/down/left/right/select)
   - Volume controls (up/down/mute)
   - Media controls (play/pause/stop/ff/rewind)
   - Home/back/menu buttons
   - Mouse/touchpad simulation
   - Keyboard text input

3. **Connection Process**
   - Initial Bluetooth pairing through system settings
   - App-level connection to paired device
   - HID profile activation

## Apple TV Control Methods

### Network-Based Control (Primary)
- **Protocol**: HTTP-based communication on port 3689
- **Discovery**: mDNS service discovery
- **Authentication**: RSA key exchange and pairing
- **Encryption**: AES encryption for commands

### Bluetooth LE HID (Secondary)
- **Technology**: Bluetooth Low Energy HID profile
- **Function**: Acts as Bluetooth LE HID Keyboard controller
- **Limitations**: Limited command set compared to network protocol

### MediaRemoteTV Protocol
- **Purpose**: Comprehensive Apple TV control
- **Capabilities**: 
  - Interface navigation
  - Keyboard commands
  - Voice commands
  - Game controller input
  - Media playbook control

## C# Implementation Strategy

### Required NuGet Packages
```
Windows.Devices.Bluetooth - Bluetooth device access
Windows.Devices.HumanInterfaceDevice - HID device simulation
System.Net.NetworkInformation - Network discovery
System.Net.Sockets - TCP/UDP communication
```

### Architecture Components

#### 1. Bluetooth HID Controller
```csharp
public interface IBluetoothHIDController
{
    Task<bool> StartAdvertisingAsync();
    Task<bool> ConnectToDeviceAsync(string deviceId);
    Task<bool> SendKeyEventAsync(HIDKeyCode keyCode);
    Task<bool> SendMouseEventAsync(int deltaX, int deltaY, bool leftClick);
    Task DisconnectAsync();
}
```

#### 2. Android TV Controller
```csharp
public class AndroidTVBluetoothController : IBluetoothHIDController
{
    // HID descriptor for Android TV remote
    // Key mapping for navigation and media controls
    // Connection management
}
```

#### 3. Apple TV Controller
```csharp
public class AppleTVNetworkController : INetworkDeviceController
{
    // mDNS discovery for Apple TV devices
    // HTTP-based command sending
    // RSA pairing and authentication
    // AES encryption for secure communication
}
```

### HID Key Mappings

#### Android TV Remote Keys
```csharp
public enum AndroidTVKeys
{
    DPadUp = 0x52,
    DPadDown = 0x51,
    DPadLeft = 0x50,
    DPadRight = 0x4F,
    DPadCenter = 0x28,
    Back = 0x29,
    Home = 0x4A,
    Menu = 0x76,
    VolumeUp = 0x80,
    VolumeDown = 0x81,
    VolumeMute = 0x7F,
    PlayPause = 0xCD,
    FastForward = 0xB3,
    Rewind = 0xB4
}
```

## Platform Limitations

### Windows Implementation
- Bluetooth HID peripheral mode support limited
- May require Windows 10/11 with specific Bluetooth chipsets
- Alternative: USB HID device simulation

### Cross-Platform Considerations
- Android TV: Full Bluetooth HID support
- Apple TV: Network protocol preferred over Bluetooth
- Windows compatibility may require fallback methods

## Integration with Zapper

### Device Model Extensions
```csharp
// Add to ConnectionType enum
AndroidTVBluetooth,
AppleTVNetwork,

// Add to Device model
public string? BluetoothAddress { get; set; }
public string? AppleTVPairingKey { get; set; }
public bool SupportsMouseInput { get; set; }
public bool SupportsKeyboardInput { get; set; }
```

### Command Extensions
```csharp
// Add to CommandType enum
MouseMove,
MouseClick,
KeyboardInput,
TouchpadGesture,

// Add to DeviceCommand model
public int? MouseDeltaX { get; set; }
public int? MouseDeltaY { get; set; }
public string? KeyboardText { get; set; }
```

## Security Considerations

### Bluetooth Security
- Encryption during pairing process
- Authentication via PIN or passkey
- Limited range (typically 10 meters)

### Network Security (Apple TV)
- RSA key exchange for initial pairing
- AES encryption for all subsequent commands
- Device-specific pairing keys

## Implementation Priority

1. **Phase 1**: Android TV Bluetooth HID support
   - Basic navigation controls (D-pad, back, home)
   - Volume controls
   - Media playback controls

2. **Phase 2**: Enhanced Android TV support
   - Mouse/touchpad simulation
   - Keyboard text input
   - Custom gesture support

3. **Phase 3**: Apple TV network protocol
   - mDNS device discovery
   - HTTP-based command interface
   - Pairing and authentication

4. **Phase 4**: Apple TV Bluetooth LE HID
   - Basic keyboard simulation
   - Limited navigation support

## Next Steps
1. Implement IBluetoothHIDController interface
2. Create Android TV HID descriptor and key mappings
3. Add Bluetooth device discovery and pairing
4. Integrate with existing Zapper device management
5. Create API endpoints for Bluetooth device control