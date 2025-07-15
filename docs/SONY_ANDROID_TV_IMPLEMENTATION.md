# Sony Android TV Implementation Guide

## Overview
This document provides detailed specifications and implementation guidance for adding Sony Android TV support to the Zapper universal remote control system.

## Sony Android TV Technologies

### 1. Sony Bravia Professional Display API (REST)
Sony Android TVs support a REST-based API for control and status monitoring.

**Key Features:**
- Power control
- Volume control
- Input switching
- App launching
- System information retrieval

**API Endpoints:**
- Base URL: `http://<tv-ip>/sony/`
- Authentication: Pre-Shared Key (PSK) or Cookie-based

### 2. Android TV Remote Service v2 (Protocol Buffers)
Google's remote protocol for Android TV devices using Protocol Buffers over TLS.

**Features:**
- Virtual remote control
- Text input
- Voice commands
- App control

**Requirements:**
- TLS connection on port 6467
- Pairing process required
- Certificate validation

### 3. DIAL (Discovery and Launch)
Standard protocol for discovering and launching apps on smart TVs.

**Features:**
- App discovery
- App launching
- Basic app control

### 4. Google Cast Protocol
For casting content and basic control.

**Features:**
- Media casting
- Volume control
- Basic playback control

## Implementation Architecture

```
Zapper.Device.SonyAndroidTV/
├── SonyAndroidTVController.cs       # Main device controller
├── BraviaApiClient.cs              # REST API client
├── AndroidTVRemoteClient.cs        # Protocol Buffers client
├── SonyTVDiscovery.cs             # SSDP/mDNS discovery
├── Models/
│   ├── BraviaCommand.cs
│   ├── BraviaResponse.cs
│   └── AndroidTVRemoteMessage.cs
└── ServiceCollectionExtensions.cs
```

## Detailed Implementation Plan

### Phase 1: Basic Control via Bravia API

#### 1.1 Discovery Implementation
```csharp
public class SonyTVDiscovery : IDeviceDiscovery
{
    // SSDP discovery for Sony TVs
    // Look for: urn:schemas-sony-com:service:ScalarWebAPI:1
    // mDNS discovery for _googlecast._tcp
}
```

#### 1.2 Bravia API Client
```csharp
public class BraviaApiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _psk;
    
    public async Task<bool> PowerOnAsync()
    {
        var request = new
        {
            method = "setPowerStatus",
            id = 1,
            @params = new[] { new { status = true } },
            version = "1.0"
        };
        
        return await SendCommandAsync("system", request);
    }
    
    public async Task<bool> SetVolumeAsync(int volume)
    {
        var request = new
        {
            method = "setAudioVolume",
            id = 1,
            @params = new[] { new { target = "speaker", volume = volume.ToString() } },
            version = "1.0"
        };
        
        return await SendCommandAsync("audio", request);
    }
}
```

#### 1.3 Authentication
```csharp
// Pre-Shared Key authentication
httpClient.DefaultRequestHeaders.Add("X-Auth-PSK", psk);

// Cookie-based authentication for newer models
// Requires on-screen PIN confirmation
```

### Phase 2: Android TV Remote Protocol

#### 2.1 Pairing Process
```csharp
public class AndroidTVPairing
{
    public async Task<PairingResult> PairAsync(string tvIpAddress)
    {
        // 1. Connect via TLS to port 6467
        // 2. Send pairing request
        // 3. Display pairing code on TV
        // 4. User confirms code
        // 5. Store certificate for future connections
    }
}
```

#### 2.2 Remote Control Implementation
```csharp
public class AndroidTVRemoteClient
{
    public async Task SendKeyAsync(AndroidTVKey key)
    {
        var message = new RemoteMessage
        {
            Type = MessageType.KeyEvent,
            KeyEvent = new KeyEvent
            {
                Keycode = (int)key,
                Action = KeyAction.Down
            }
        };
        
        await SendMessageAsync(message);
    }
}
```

### Phase 3: Advanced Features

#### 3.1 App Control
```csharp
public async Task<List<InstalledApp>> GetInstalledAppsAsync()
{
    var request = new
    {
        method = "getApplicationList",
        id = 1,
        @params = new object[] { },
        version = "1.0"
    };
    
    return await SendCommandAsync<List<InstalledApp>>("appControl", request);
}

public async Task LaunchAppAsync(string appUri)
{
    // Use DIAL protocol or Bravia API
}
```

#### 3.2 Status Monitoring
```csharp
public async Task<TVStatus> GetStatusAsync()
{
    // Get power status
    // Get current input
    // Get volume level
    // Get playing content info
}
```

## Device Registration

```csharp
public class SonyAndroidTVController : IDeviceController
{
    public DeviceCapabilities GetCapabilities()
    {
        return new DeviceCapabilities
        {
            SupportsPower = true,
            SupportsVolume = true,
            SupportsInput = true,
            SupportsApps = true,
            SupportsTextInput = true,
            RequiresPairing = true,
            ConnectionTypes = new[]
            {
                ConnectionType.NetworkTcp,
                ConnectionType.NetworkWebSocket
            }
        };
    }
}
```

## Configuration Model

```csharp
public class SonyAndroidTVConfiguration
{
    public string IpAddress { get; set; }
    public string? PreSharedKey { get; set; }
    public string? DeviceName { get; set; }
    public bool UseAndroidTVRemote { get; set; } = true;
    public byte[]? PairingCertificate { get; set; }
    public string? MacAddress { get; set; }
}
```

## User Interface Integration

### Discovery Wizard
1. Scan network for Sony Android TVs
2. Display found devices with model information
3. Allow manual IP entry
4. Guide through PSK setup or pairing process

### Control Interface
```typescript
interface SonyAndroidTVControls {
  // Basic controls
  power: () => Promise<void>;
  volumeUp: () => Promise<void>;
  volumeDown: () => Promise<void>;
  mute: () => Promise<void>;
  
  // Navigation
  up: () => Promise<void>;
  down: () => Promise<void>;
  left: () => Promise<void>;
  right: () => Promise<void>;
  enter: () => Promise<void>;
  back: () => Promise<void>;
  home: () => Promise<void>;
  
  // Media controls
  play: () => Promise<void>;
  pause: () => Promise<void>;
  stop: () => Promise<void>;
  
  // Apps
  launchApp: (appId: string) => Promise<void>;
  getApps: () => Promise<App[]>;
  
  // Input
  sendText: (text: string) => Promise<void>;
  switchInput: (input: string) => Promise<void>;
}
```

## Testing Strategy

### Unit Tests
```csharp
[Fact]
public async Task PowerOn_SendsCorrectBraviaCommand()
{
    // Test Bravia API command formatting
}

[Fact]
public async Task PairingProcess_CompletesSuccessfully()
{
    // Test Android TV pairing flow
}
```

### Integration Tests
- Test with Sony TV emulator/simulator
- Mock Bravia API responses
- Test network discovery
- Test error handling

### Manual Testing Checklist
- [ ] Discovery finds Sony Android TVs on network
- [ ] PSK authentication works
- [ ] Pairing process completes
- [ ] Basic controls (power, volume, navigation)
- [ ] App launching
- [ ] Text input
- [ ] Multiple TV support
- [ ] Connection recovery

## Security Considerations

1. **PSK Storage**: Encrypt pre-shared keys in database
2. **Certificate Validation**: Properly validate TLS certificates
3. **Network Security**: Use HTTPS where possible
4. **Access Control**: Limit TV control to authorized users

## Performance Optimization

1. **Connection Pooling**: Reuse HTTP/TLS connections
2. **Command Queuing**: Batch rapid commands
3. **Status Caching**: Cache TV status with short TTL
4. **Discovery Caching**: Cache discovered devices

## Known Sony Android TV Models

### Bravia Series
- X950H, X900H (2020)
- X950G, X850G (2019)
- A8H OLED series
- A9G OLED series

### API Versions
- v1.0: Basic control (2015-2017 models)
- v1.1: Extended app control (2018-2019 models)
- v1.2: Enhanced status (2020+ models)

## Troubleshooting Guide

### Common Issues
1. **Discovery fails**: Check firewall, ensure TV network settings
2. **PSK rejected**: Verify PSK in TV settings
3. **Pairing fails**: Reset pairing on TV
4. **Commands delayed**: Check network latency
5. **App launch fails**: Verify app is installed

### Debug Logging
```csharp
_logger.LogDebug("Sending Bravia command: {Command}", JsonSerializer.Serialize(command));
_logger.LogDebug("Bravia response: {Response}", response);
```

## References

- [Sony Bravia Professional Display API](https://pro-bravia.sony.net/develop/integrate/rest-api/)
- [Android TV Remote Service Protocol](https://github.com/Aymkdn/assistant-freebox-cloud/wiki/Google-TV-(aka-Android-TV)-Remote-Control-(v2))
- [DIAL Protocol Specification](http://www.dial-multiscreen.org/)
- [Google Cast Protocol](https://developers.google.com/cast/)