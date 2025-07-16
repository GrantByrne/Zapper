# PlayStation Integration

## Overview

PlayStation integration enables Zapper to control PlayStation 4 and PlayStation 5 consoles over the network using a custom implementation of the Remote Play protocol. This integration provides full remote control functionality, including navigation, media playback, and game controller buttons, without requiring Bluetooth pairing or physical connection.

## Supported Devices

### PlayStation 4
- **PlayStation 4** (Original)
- **PlayStation 4 Slim**
- **PlayStation 4 Pro**

### PlayStation 5
- **PlayStation 5** (Standard Edition)
- **PlayStation 5 Digital Edition**
- **PlayStation 5 Slim**

### Compatibility Notes
- Both PS4 and PS5 use the same control protocol
- Remote Play must be enabled on the console
- Console must be on the same network as Zapper

## Features

### Current Features
- **Automatic Discovery**: Network scanning to find PlayStation consoles
- **Power Control**: Power on/off functionality
- **Navigation**: D-pad navigation (up, down, left, right)
- **Button Control**: All PlayStation controller buttons
- **Media Control**: Play/pause, stop, fast forward, rewind
- **System Navigation**: PS button, back, options, share
- **Connection Management**: Persistent connections with tracking
- **Custom Commands**: Support for game-specific controls

### Future Features (Planned)
- Wake-on-LAN support
- Game library browsing
- Screenshot/video capture control
- Voice command integration
- Multi-console management
- Activity switching

## Network Requirements

### Console Configuration
1. **Enable Remote Play**
   - PS5: Settings → System → Remote Play → Enable Remote Play
   - PS4: Settings → Remote Play Connection Settings → Enable Remote Play

2. **Network Settings**
   - Console and Raspberry Pi must be on same network
   - No firewall blocking between devices
   - Static IP recommended for consoles

### Ports
- **Port 9295**: Remote Play protocol (TCP/UDP)
- Both TCP and UDP protocols are used

### Discovery Method
- Network scanning of all IPs in subnet
- Checks port 9295 for Remote Play service
- Automatic detection of console type

## Discovery Process

### Automatic Discovery

1. Navigate to **Devices** → **Add Device**
2. Select **PlayStation** as connection type
3. Click **Discover PlayStation Consoles**
4. Wait for network scan (10-30 seconds)
5. Select your console from discovered list

The discovery process:
- Scans all IP addresses in local subnet
- Pings each IP for availability
- Checks port 9295 for Remote Play service
- Identifies PlayStation consoles automatically

### Manual Configuration

If automatic discovery fails:

```json
{
  "name": "Living Room PS5",
  "type": "PlayStation",
  "connectionType": "Network",
  "ipAddress": "192.168.1.75",
  "port": 9295,
  "model": "PlayStation 5"
}
```

## Configuration

### Discovery Settings

```json
{
  "PlayStation": {
    "Discovery": {
      "Timeout": 10,
      "ScanPort": 9295,
      "PingTimeout": 1000
    }
  }
}
```

### Connection Settings

```json
{
  "PlayStation": {
    "Connection": {
      "CommandTimeout": 5000,
      "KeepAlive": true,
      "ReconnectOnFailure": true
    }
  }
}
```

## Available Commands

### Navigation Controls

| Command | PlayStation Button | Description |
|---------|-------------------|-------------|
| `up` | D-Pad Up | Navigate up in menus |
| `down` | D-Pad Down | Navigate down in menus |
| `left` | D-Pad Left | Navigate left in menus |
| `right` | D-Pad Right | Navigate right in menus |

### Action Buttons

| Command | PlayStation Button | Description |
|---------|-------------------|-------------|
| `cross` | X (Cross) | Select/Confirm |
| `circle` | O (Circle) | Back/Cancel |
| `square` | □ (Square) | Context action |
| `triangle` | △ (Triangle) | Menu/Options |

### System Buttons

| Command | PlayStation Button | Description |
|---------|-------------------|-------------|
| `ps` | PS Button | PlayStation home menu |
| `options` | Options | Game/app options menu |
| `share` | Share/Create | Share or create content |
| `touchpad` | Touchpad | Touchpad press |

### Shoulder Buttons

| Command | PlayStation Button | Description |
|---------|-------------------|-------------|
| `l1` | L1 | Left shoulder button |
| `r1` | R1 | Right shoulder button |
| `l2` | L2 | Left trigger |
| `r2` | R2 | Right trigger |

### Media Controls

| Command | PlayStation Button | Description |
|---------|-------------------|-------------|
| `play_pause` | Play/Pause | Toggle playback |
| `stop` | Stop | Stop playback |
| `l1` | L1 | Rewind (in media apps) |
| `r1` | R1 | Fast forward (in media apps) |

### Power Control

| Command | Description |
|---------|-------------|
| `power` | Toggle power state |
| `power_on` | Turn on console (if supported) |
| `power_off` | Enter rest mode |

## Button Mapping

### Zapper Commands to PlayStation Buttons

```json
{
  "mapping": {
    "Power": "power",
    "Menu": "ps",
    "Back": "back",
    "DirectionalUp": "up",
    "DirectionalDown": "down",
    "DirectionalLeft": "left",
    "DirectionalRight": "right",
    "Ok": "cross",
    "PlayPause": "play_pause",
    "Stop": "stop",
    "FastForward": "r1",
    "Rewind": "l1"
  }
}
```

## Protocol Details

### Communication Methods

#### TCP Commands
Used for button presses and navigation:
```json
{
  "type": "button",
  "button": "cross"
}
```

#### UDP Commands
Used for power control and ping:
```json
{
  "type": "power_on"
}
```

### Connection Flow
1. Establish TCP connection on port 9295
2. Send JSON-formatted commands
3. Maintain connection for subsequent commands
4. Use UDP for power and discovery

## Advanced Features

### Custom Commands

Define custom button combinations for games:

```json
{
  "customCommands": {
    "sprint": {
      "button": "l3",
      "description": "Left stick click for sprint"
    },
    "map": {
      "button": "touchpad",
      "description": "Open map in games"
    }
  }
}
```

### Power Management

The power command intelligently toggles state:
1. Tests connection with ping
2. If console responds: sends power off
3. If no response: sends power on via UDP

### Connection Persistence

Zapper maintains connections to reduce latency:
- Connections tracked per IP address
- Automatic cleanup of stale connections
- Reconnection on command failure

## Troubleshooting

### Discovery Issues

#### No Consoles Found
1. **Verify Remote Play Enabled**
   - Check console settings
   - Ensure "Enable Remote Play" is on

2. **Network Configuration**
   - Consoles and Pi on same subnet
   - No VLAN separation
   - Check router isolation settings

3. **Manual Discovery Test**
   ```bash
   nc -zv [CONSOLE-IP] 9295
   ```

#### Partial Discovery
- Some consoles in rest mode won't respond
- Wake console manually first time
- Use manual configuration for reliability

### Connection Problems

#### Connection Refused
1. **Remote Play Settings**
   - Re-enable Remote Play
   - Restart console

2. **Network Issues**
   ```bash
   ping [CONSOLE-IP]
   telnet [CONSOLE-IP] 9295
   ```

#### Commands Not Working
1. **Console State**
   - Ensure console is not in rest mode
   - Check if another device is using Remote Play

2. **Connection Test**
   - Use test connection feature
   - Monitor logs for errors

### Power Control Issues

#### Console Won't Turn On
- Power on via UDP requires specific console settings
- Some models don't support network wake
- Use console's power button for initial setup

#### Console Won't Turn Off
- Power command puts console in rest mode
- Full shutdown requires manual intervention
- Check rest mode settings on console

## API Usage

### Discover PlayStation Consoles

```http
POST /api/devices/discover/playstation
Content-Type: application/json

{
  "timeout": 15
}
```

Response:
```json
{
  "devices": [
    {
      "name": "PlayStation (192.168.1.75)",
      "ipAddress": "192.168.1.75",
      "model": "PlayStation 5",
      "isOnline": true
    }
  ]
}
```

### Send Command

```http
POST /api/devices/{deviceId}/commands
Content-Type: application/json

{
  "command": "cross"
}
```

### Send Custom Command

```http
POST /api/devices/{deviceId}/commands
Content-Type: application/json

{
  "command": "custom",
  "networkPayload": "triangle"
}
```

### Test Connection

```http
GET /api/devices/{deviceId}/test
```

Response:
```json
{
  "connected": true,
  "latency": 5
}
```

## Security Considerations

### Network Security
- Remote Play protocol is unencrypted
- Use isolated network for gaming devices
- Consider firewall rules for port 9295

### Access Control
- No authentication in Remote Play protocol
- Restrict network access to trusted devices
- Monitor for unauthorized connections

## Integration Tips

### Performance Optimization
1. **Static IP Assignment**
   - Assign static IPs to consoles
   - Faster discovery and connection

2. **Connection Caching**
   - Maintain persistent connections
   - Reduce command latency

3. **Network Priority**
   - Configure QoS for gaming traffic
   - Minimize network interference

### Activity Examples

#### Gaming Mode
```json
{
  "name": "Gaming Mode",
  "commands": [
    { "device": "ps5", "command": "power_on" },
    { "delay": 5000 },
    { "device": "tv", "command": "hdmi2" },
    { "device": "soundbar", "command": "game_mode" }
  ]
}
```

#### Media Streaming
```json
{
  "name": "Netflix on PlayStation",
  "commands": [
    { "device": "ps5", "command": "power_on" },
    { "delay": 5000 },
    { "device": "ps5", "command": "ps" },
    { "delay": 1000 },
    { "device": "tv", "command": "hdmi2" }
  ]
}
```

## Known Limitations

### Current Implementation
- No Wake-on-LAN support
- Cannot launch specific games/apps
- No screenshot or video capture control
- No controller vibration feedback
- No microphone/headset control
- Limited power on reliability
- No support for multiple user profiles

### Protocol Limitations
- Unencrypted communication
- No official API documentation
- Limited to local network only
- Cannot bypass parental controls

### Future Enhancements
- Wake-on-LAN implementation
- Game library integration
- Voice command support
- Multi-console management
- Enhanced power control
- Activity state detection