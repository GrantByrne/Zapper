# WebOS Integration

## Overview

WebOS integration provides native control of LG Smart TVs through their built-in WebSocket API. This integration offers advanced features beyond traditional IR control, including app launching, input switching, volume control with feedback, and access to TV settings.

## Supported Devices

### Compatible TVs
- **LG OLED TVs** (2016 and newer)
- **LG NanoCell TVs** (2017 and newer)
- **LG UHD TVs** (2016 and newer)
- **LG QNED TVs** (all models)

### WebOS Versions
- **WebOS 3.0** and newer (full support)
- **WebOS 2.0** (limited support)
- **WebOS 1.0** (basic support only)

## Features

### Basic Control
- Power on/off (with Wake-on-LAN)
- Volume control with real-time feedback
- Channel changing
- Input source switching
- Mute toggle

### Advanced Features
- Launch apps (Netflix, YouTube, etc.)
- Control media playback
- Access TV settings
- Display notifications on TV
- Screenshot capture
- Get current app/input status

### Real-time Feedback
- Current volume level
- Active input source
- Running application
- Power state
- Channel information

## Network Requirements

### TV Configuration
1. **Enable Network Control**
   - Settings → All Settings → Network → LG Connect Apps
   - Enable "LG Connect Apps" or "Mobile TV On"

2. **Note TV Information**
   - IP address (static IP recommended)
   - MAC address (for Wake-on-LAN)

### Network Setup
- TV and Raspberry Pi must be on same network
- Port 3000 must be accessible (WebSocket)
- Port 9 for Wake-on-LAN (optional)

## Discovery Process

### Automatic Discovery

Zapper uses SSDP (Simple Service Discovery Protocol) to find WebOS TVs:

1. Navigate to **Devices** → **Add Device**
2. Select **WebOS** as connection type
3. Click **Discover TVs**
4. Select your TV from the list
5. Accept pairing prompt on TV

### Manual Configuration

If automatic discovery fails:

```json
{
  "name": "LG OLED TV",
  "type": "Television",
  "connectionType": "WebOS",
  "ipAddress": "192.168.1.100",
  "macAddress": "AA:BB:CC:DD:EE:FF",
  "port": 3000
}
```

## Pairing Process

### First-Time Connection

1. **Initiate Connection**
   - Zapper connects to TV WebSocket
   - Pairing request sent automatically

2. **TV Prompt**
   - Notification appears on TV screen
   - Select "Accept" using TV remote

3. **Store Credentials**
   - Zapper saves pairing key
   - Future connections automatic

### Pairing Configuration

```json
{
  "webOS": {
    "clientKey": "generated-after-pairing",
    "pairingTimeout": 60,
    "reconnectDelay": 5000
  }
}
```

## Available Commands

### Power Control

```json
{
  "power": {
    "on": "ssap://system/turnOn",
    "off": "ssap://system/turnOff",
    "status": "ssap://com.webos.service.tvpower/power/getPowerState"
  }
}
```

### Volume Control

```json
{
  "volume": {
    "up": "ssap://audio/volumeUp",
    "down": "ssap://audio/volumeDown",
    "set": "ssap://audio/setVolume",
    "mute": "ssap://audio/setMute",
    "status": "ssap://audio/getVolume"
  }
}
```

### Input Control

```json
{
  "input": {
    "list": "ssap://tv/getExternalInputList",
    "switch": "ssap://tv/switchInput",
    "current": "ssap://com.webos.applicationManager/getForegroundAppInfo"
  }
}
```

### App Control

```json
{
  "apps": {
    "list": "ssap://com.webos.applicationManager/listApps",
    "launch": "ssap://com.webos.applicationManager/launch",
    "close": "ssap://com.webos.applicationManager/close",
    "netflix": "netflix",
    "youtube": "youtube.leanback.v4",
    "prime": "amazon",
    "disney": "com.disney.disneyplus-prod"
  }
}
```

### Media Control

```json
{
  "media": {
    "play": "ssap://media.controls/play",
    "pause": "ssap://media.controls/pause",
    "stop": "ssap://media.controls/stop",
    "rewind": "ssap://media.controls/rewind",
    "fastForward": "ssap://media.controls/fastForward"
  }
}
```

## Advanced Features

### Wake-on-LAN

Enable TV power on over network:

```json
{
  "wakeOnLan": {
    "enabled": true,
    "broadcast": "192.168.1.255",
    "port": 9,
    "packets": 3,
    "interval": 100
  }
}
```

### Custom Notifications

Display messages on TV:

```http
POST /api/devices/{deviceId}/notify
Content-Type: application/json

{
  "message": "Doorbell pressed",
  "iconUrl": "http://example.com/icon.png",
  "duration": 5000
}
```

### TV Settings Access

```json
{
  "settings": {
    "picture": "ssap://settings/getSystemSettings",
    "sound": "ssap://audio/getSoundOutput",
    "network": "ssap://com.webos.service.config/getConfigs"
  }
}
```

### Mouse/Pointer Control

```json
{
  "pointer": {
    "show": "ssap://com.webos.service.networkinput/getPointerInputSocket",
    "move": { "dx": 10, "dy": 10 },
    "click": "LEFT",
    "scroll": { "dx": 0, "dy": 5 }
  }
}
```

## Configuration

### Default Settings

```json
{
  "WebOS": {
    "ConnectionTimeout": 10000,
    "CommandTimeout": 5000,
    "KeepAliveInterval": 30000,
    "AutoReconnect": true,
    "MaxReconnectAttempts": 5,
    "ReconnectDelay": 5000
  }
}
```

### Per-Device Settings

```json
{
  "devices": {
    "lgTv": {
      "ipAddress": "192.168.1.100",
      "macAddress": "AA:BB:CC:DD:EE:FF",
      "clientKey": "stored-after-pairing",
      "powerOnDelay": 15000,
      "inputSwitchDelay": 500
    }
  }
}
```

## Troubleshooting

### TV Not Discovered

1. **Check Network Settings**
   ```bash
   ping [TV-IP-ADDRESS]
   ```

2. **Verify LG Connect Apps Enabled**
   - Settings → Network → LG Connect Apps → On

3. **Check Firewall**
   - Allow UDP port 1900 (SSDP)
   - Allow TCP port 3000 (WebSocket)

### Connection Refused

1. **Re-pair Device**
   - Delete device from Zapper
   - Reset TV network settings
   - Re-add and pair device

2. **Check TV Sleep Mode**
   - Disable "Quick Start+"
   - Enable "Wake on LAN"

### Commands Not Working

1. **Verify WebOS Version**
   - Older versions have limited commands
   - Check TV model compatibility

2. **Monitor WebSocket Connection**
   ```bash
   wscat -c ws://[TV-IP]:3000
   ```

### Intermittent Disconnections

1. **Network Stability**
   - Use wired connection if possible
   - Assign static IP to TV

2. **TV Power Settings**
   - Disable auto power off
   - Disable eco mode

## API Usage

### Get TV Status

```http
GET /api/devices/{deviceId}/status
```

Response:
```json
{
  "connected": true,
  "powerState": "on",
  "currentApp": "netflix",
  "currentInput": "HDMI1",
  "volume": 25,
  "muted": false
}
```

### Launch App

```http
POST /api/devices/{deviceId}/commands
Content-Type: application/json

{
  "command": "launchApp",
  "appId": "netflix"
}
```

### Switch Input

```http
POST /api/devices/{deviceId}/commands
Content-Type: application/json

{
  "command": "switchInput",
  "inputId": "HDMI_1"
}
```

### Set Volume

```http
POST /api/devices/{deviceId}/commands
Content-Type: application/json

{
  "command": "setVolume",
  "volume": 30
}
```

## Security Considerations

### Network Security
- Use isolated VLAN for IoT devices
- Enable TV firmware auto-updates
- Monitor unauthorized access attempts

### Authentication
- Pairing key stored encrypted
- API endpoints require authentication
- Rate limiting on commands

## Integration Tips

### Optimal Performance
1. **Use WebSocket keep-alive**
2. **Cache TV capabilities**
3. **Batch commands when possible**
4. **Handle reconnection gracefully**

### Activity Examples

#### Movie Mode
```json
{
  "name": "Movie Mode",
  "commands": [
    { "device": "lgTv", "command": "powerOn" },
    { "delay": 15000 },
    { "device": "lgTv", "command": "switchInput", "input": "HDMI_1" },
    { "delay": 500 },
    { "device": "lgTv", "command": "setPictureMode", "mode": "cinema" }
  ]
}
```

#### Gaming Mode
```json
{
  "name": "Gaming",
  "commands": [
    { "device": "lgTv", "command": "powerOn" },
    { "delay": 15000 },
    { "device": "lgTv", "command": "switchInput", "input": "HDMI_2" },
    { "delay": 500 },
    { "device": "lgTv", "command": "setGameMode", "enabled": true }
  ]
}
```

## Known Limitations

### WebOS 2.0
- No app launching
- Limited settings access
- Basic control only

### Network Control
- Cannot power on without Wake-on-LAN
- Some settings require on-screen confirmation
- HDR mode switching may be limited

### Regional Differences
- Available apps vary by region
- Some features geo-restricted
- Input names may differ