# Sonos Integration

## Overview

Sonos integration enables Zapper to control Sonos multi-room audio systems over the network using UPnP (Universal Plug and Play) and SOAP protocols. This integration provides automatic discovery of Sonos devices and control of playback, volume, and basic transport functions.

## Supported Devices

### Sonos Speakers
- **Sonos One** (Gen 1 & 2)
- **Sonos One SL**
- **Sonos Play:1**
- **Sonos Play:3**
- **Sonos Play:5** (Gen 1 & 2)
- **Sonos Five**

### Sonos Soundbars
- **Sonos Beam** (Gen 1 & 2)
- **Sonos Arc**
- **Sonos Ray**
- **Playbar**
- **Playbase**

### Portable Speakers
- **Sonos Move**
- **Sonos Move 2**
- **Sonos Roam**
- **Sonos Roam SL**

### Components
- **Sonos Amp**
- **Sonos Connect**
- **Sonos Connect:Amp**
- **Sonos Port**

### Legacy Devices
- **Sonos ZonePlayer** series
- All S1 and S2 compatible devices

## Features

### Current Features
- **Automatic Discovery**: Find all Sonos devices on the network via SSDP
- **Basic Playback Control**: Play, pause, stop functionality
- **Volume Control**: Set absolute volume, increase/decrease volume
- **Mute Control**: Mute and unmute speakers
- **Connection Management**: Maintain connections to multiple Sonos devices
- **Transport State**: Detect current playback state

### Future Features (Planned)
- Group and stereo pair management
- Music service integration (Spotify, Apple Music, etc.)
- Playlist and queue management
- Room/zone coordination
- Advanced EQ settings
- Sleep timers

## Network Requirements

### Network Configuration
- Sonos devices and Raspberry Pi must be on the same network
- Multicast/broadcast must be enabled for SSDP discovery
- No VLAN isolation between Zapper and Sonos devices

### Ports
- **Port 1400**: HTTP control endpoint (TCP)
- **Port 1900**: SSDP discovery (UDP multicast)

### Discovery
- Uses multicast address: `239.255.255.250:1900`
- Searches for: `urn:schemas-upnp-org:device:ZonePlayer:1`

## Discovery Process

### Automatic Discovery

1. Navigate to **Devices** → **Add Device**
2. Select **Sonos** as connection type
3. Click **Discover Sonos Devices**
4. Select devices from the discovered list
5. Devices are automatically configured

The discovery process:
- Sends SSDP M-SEARCH request
- Listens for responses from Sonos devices
- Fetches device details from `/xml/device_description.xml`
- Extracts device name, model, and network information

### Manual Configuration

If automatic discovery fails:

```json
{
  "name": "Living Room Sonos",
  "type": "AudioDevice",
  "connectionType": "Sonos",
  "ipAddress": "192.168.1.50",
  "port": 1400,
  "model": "Sonos One"
}
```

## Configuration

### Discovery Settings

```json
{
  "Sonos": {
    "Discovery": {
      "Timeout": 10,
      "MaxRetries": 3,
      "MulticastTTL": 2
    }
  }
}
```

### Connection Settings

```json
{
  "Sonos": {
    "Connection": {
      "CommandTimeout": 5000,
      "KeepAlive": true,
      "MaxConcurrentDevices": 32
    }
  }
}
```

## Available Commands

### Playback Control

```json
{
  "commands": {
    "play": "Play current queue or resume playback",
    "pause": "Pause playback",
    "stop": "Stop playback and clear position",
    "playPause": "Toggle between play and pause"
  }
}
```

### Volume Control

```json
{
  "volume": {
    "setVolume": "Set absolute volume (0-100)",
    "volumeUp": "Increase volume by 5",
    "volumeDown": "Decrease volume by 5",
    "mute": "Mute speaker",
    "unmute": "Unmute speaker"
  }
}
```

### Power Control

Note: Sonos speakers don't have traditional power on/off. The power command maps to play/pause:
- **Power On**: Starts playback if content is queued
- **Power Off**: Pauses playback

## SOAP/UPnP Protocol Details

### Service Endpoints

#### AVTransport Service
- **Endpoint**: `/MediaRenderer/AVTransport/Control`
- **Service Type**: `urn:schemas-upnp-org:service:AVTransport:1`
- **Actions**: Play, Pause, Stop, GetTransportInfo

#### RenderingControl Service
- **Endpoint**: `/MediaRenderer/RenderingControl/Control`
- **Service Type**: `urn:schemas-upnp-org:service:RenderingControl:1`
- **Actions**: SetVolume, GetVolume, SetMute, GetMute

### Example SOAP Request

```xml
POST /MediaRenderer/AVTransport/Control HTTP/1.1
Host: 192.168.1.50:1400
Content-Type: text/xml; charset="utf-8"
SOAPAction: "urn:schemas-upnp-org:service:AVTransport:1#Play"

<s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/">
  <s:Body>
    <u:Play xmlns:u="urn:schemas-upnp-org:service:AVTransport:1">
      <InstanceID>0</InstanceID>
      <Speed>1</Speed>
    </u:Play>
  </s:Body>
</s:Envelope>
```

## Advanced Features

### Connection Management

Zapper maintains persistent connections to discovered Sonos devices:
- Automatic reconnection on network interruption
- Connection pooling for multiple devices
- Concurrent command execution

### State Detection

Before executing power commands, Zapper checks the current transport state:
```csharp
// GetTransportInfo returns: PLAYING, PAUSED_PLAYBACK, STOPPED
if (state == "PLAYING") {
    // Execute pause command
} else {
    // Execute play command
}
```

## Troubleshooting

### Discovery Issues

#### No Devices Found
1. **Check Network Configuration**
   - Ensure devices are on same subnet
   - Verify multicast is not blocked
   ```bash
   ping 239.255.255.250
   ```

2. **Check Sonos System**
   - Ensure Sonos devices are powered on
   - Check Sonos app can see devices
   - Restart Sonos devices if needed

3. **Firewall Settings**
   - Allow UDP port 1900 (SSDP)
   - Allow TCP port 1400 (Control)

#### Partial Discovery
- Some routers block SSDP multicast
- Try manual configuration for missing devices
- Check for network isolation settings

### Connection Problems

#### Connection Refused
1. **Verify IP Address**
   ```bash
   curl http://[SONOS-IP]:1400/xml/device_description.xml
   ```

2. **Check Sonos Updates**
   - Ensure Sonos firmware is up to date
   - Some old firmware versions have compatibility issues

#### Commands Not Working
1. **Check Command Support**
   - Verify device supports the command
   - Some older Sonos devices have limited functionality

2. **Monitor SOAP Responses**
   - Check for error codes in responses
   - Common errors: 701 (transition not available), 714 (illegal format)

### Volume Issues
- Volume range is 0-100
- Some Sonos devices have volume limits
- Group volume behaves differently than individual volume

## API Usage

### Discover Sonos Devices

```http
POST /api/devices/discover/sonos
Content-Type: application/json

{
  "timeout": 10
}
```

Response:
```json
{
  "devices": [
    {
      "name": "Living Room",
      "ipAddress": "192.168.1.50",
      "model": "Sonos One",
      "zone": "Living Room",
      "roomName": "Living Room",
      "serialNumber": "XX-XX-XX-XX-XX-XX:X"
    }
  ]
}
```

### Send Command

```http
POST /api/devices/{deviceId}/commands
Content-Type: application/json

{
  "command": "setVolume",
  "volume": 25
}
```

### Get Device Status

```http
GET /api/devices/{deviceId}/status
```

Response:
```json
{
  "connected": true,
  "transportState": "PLAYING",
  "volume": 25,
  "muted": false,
  "currentTrack": {
    "title": "Track Name",
    "artist": "Artist Name",
    "album": "Album Name"
  }
}
```

## Integration Tips

### Optimal Performance
1. **Cache Device Information**
   - Store discovered device details
   - Reduces discovery frequency

2. **Batch Commands**
   - Group volume changes
   - Coordinate multi-room actions

3. **Handle Network Changes**
   - Monitor for IP address changes
   - Re-discover after network events

### Activity Examples

#### Party Mode
```json
{
  "name": "Party Mode",
  "commands": [
    { "device": "livingRoomSonos", "command": "setVolume", "volume": 40 },
    { "device": "kitchenSonos", "command": "setVolume", "volume": 35 },
    { "device": "patioSonos", "command": "setVolume", "volume": 45 },
    { "device": "allSonos", "command": "play" }
  ]
}
```

#### Night Mode
```json
{
  "name": "Night Mode",
  "commands": [
    { "device": "bedroomSonos", "command": "setVolume", "volume": 15 },
    { "device": "allOtherSonos", "command": "pause" }
  ]
}
```

## Known Limitations

### Current Implementation
- No support for grouping/ungrouping speakers
- Cannot create or modify stereo pairs
- No music service integration
- No playlist or queue management
- No access to Sonos favorites
- Zone and room information not fully extracted
- No support for TV audio (optical/HDMI)
- No alarm or sleep timer control
- No EQ or audio settings adjustment

### Protocol Limitations
- UPnP discovery can be unreliable on some networks
- Some advanced features require Sonos API (not UPnP)
- Cannot wake sleeping Sonos devices

### Future Enhancements
- Implement Sonos Control API for advanced features
- Add support for music service integration
- Enable group and stereo pair management
- Add queue and playlist control
- Implement zone synchronization