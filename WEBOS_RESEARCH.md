# LG webOS TV Control Protocol Research

## Overview
LG webOS TVs can be controlled remotely using a WebSocket-based protocol. This document outlines the key findings for implementing webOS TV support in Zapper.

## Connection Protocol

### Network Discovery
- **SSDP Discovery**: TVs broadcast on UDP multicast 239.255.255.250:1900
- **Search Target**: `urn:lge-com:service:webos-second-screen:1`
- **Default WebSocket Ports**: 3000 (insecure) or 3001 (secure)
- **mDNS Hostname**: TVs broadcast as `lgsmarttv.lan`

### Prerequisites
1. TV must have "LG Connect Apps" enabled in Network settings
2. Client device must be on same network as TV
3. First connection requires user approval on TV screen

### Authentication Flow
1. Initial WebSocket connection to `ws://[TV_IP]:3000` or `wss://[TV_IP]:3001`
2. Send handshake/pairing request
3. User accepts pairing on TV screen
4. TV responds with client key
5. Store client key for future connections
6. Subsequent connections use stored key

## API Structure

### URI Scheme
All commands use the format: `ssap://[service]/[method]`

### Key Services
- `ssap://system/` - System control (power, notifications)
- `ssap://media.controls/` - Media playback control
- `ssap://tv/` - TV-specific functions (channels, inputs)
- `ssap://com.webos.applicationManager/` - App management
- `ssap://audio/` - Audio control
- `ssap://com.webos.service.networkinput/` - Mouse/keyboard input

### Message Format
```json
{
  "type": "request",
  "id": "unique_id",
  "uri": "ssap://service/method",
  "payload": { /* command parameters */ }
}
```

## Essential Commands

### Power Control
- `ssap://system/turnOff` - Turn off TV
- Wake-on-LAN for power on (WebSocket cannot wake sleeping TV)

### Volume Control
- `ssap://audio/setVolume` - Set volume level
- `ssap://audio/volumeUp` - Increase volume
- `ssap://audio/volumeDown` - Decrease volume
- `ssap://audio/setMute` - Mute/unmute

### App Control
- `ssap://com.webos.applicationManager/launch` - Launch app by ID
- `ssap://com.webos.applicationManager/getForegroundAppInfo` - Get current app
- `ssap://com.webos.applicationManager/listApps` - List installed apps

### Input Control
- `ssap://tv/switchInput` - Change input source
- `ssap://tv/getExternalInputList` - Get available inputs

### Channel Control
- `ssap://tv/channelUp` - Next channel
- `ssap://tv/channelDown` - Previous channel
- `ssap://tv/openChannel` - Change to specific channel

### Notifications
- `ssap://system.notifications/createToast` - Show toast message

## Implementation Considerations

### Security
- Newer TVs require secure WebSocket connections (`wss://`)
- Client key storage is essential for seamless reconnection
- Network isolation may prevent discovery

### Error Handling
- Connection timeouts and retries
- TV sleep/wake state management
- Network connectivity issues

### Performance
- WebSocket connection pooling
- Command queueing for rapid commands
- Subscription management for status updates

## C# Implementation Strategy

### Required NuGet Packages
- `System.Net.WebSockets.Client` - WebSocket client
- `Newtonsoft.Json` - JSON serialization
- `System.Net.NetworkInformation` - Network discovery

### Architecture Components
1. **WebOSClient** - Main WebSocket connection handler
2. **WebOSDiscovery** - SSDP-based TV discovery
3. **WebOSAuthentication** - Pairing and key management
4. **WebOSCommands** - Typed command builders
5. **WebOSDevice** - Device abstraction for Zapper

### Integration Points
- Implement `INetworkDeviceController` interface
- Add webOS device type to Device model
- Support webOS-specific properties (MAC address for WoL)
- Handle authentication tokens in device configuration

## Next Steps
1. Implement WebOSClient class with WebSocket communication
2. Add SSDP discovery for automatic TV detection
3. Create command builders for common operations
4. Integrate with existing Zapper device management
5. Add webOS device type to database models