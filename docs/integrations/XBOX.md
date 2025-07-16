# Xbox Integration

## Overview

Xbox integration enables Zapper to control Xbox One and Xbox Series consoles over the network using a custom protocol implementation. This integration provides full remote control functionality including navigation, media playback, game controller buttons, and text input capabilities. The implementation supports all Xbox consoles from the Xbox One generation onwards.

## Supported Devices

### Xbox One Generation
- **Xbox One** (Original, 2013)
- **Xbox One S** (2016)
- **Xbox One X** (2017)
- **Xbox One S All-Digital Edition** (2019)

### Xbox Series Generation
- **Xbox Series X** (2020)
- **Xbox Series S** (2020)

### Compatibility
- All consoles use the same control protocol
- Requires network connectivity (Wi-Fi or Ethernet)
- SmartGlass/Companion app features must be enabled

## Features

### Current Features
- **Automatic Discovery**: UDP broadcast discovery of Xbox consoles
- **Power Control**: Power on with Live ID, power off/sleep
- **Navigation**: Full D-pad navigation control
- **Button Control**: Complete Xbox controller button mapping
- **Media Control**: Play/pause, stop, fast forward, rewind
- **System Control**: Xbox/Guide button, back, view, menu
- **Text Input**: Send text for searches and login
- **Number Input**: Direct number key support
- **Connection Management**: Persistent connections with tracking
- **Multi-Console Support**: Control multiple Xbox consoles

### Future Features (Planned)
- Game/app launching
- Screenshot and video capture control
- Party chat management
- Achievement notifications
- Store navigation
- Profile switching
- Voice command passthrough

## Network Requirements

### Console Configuration
1. **Enable Network Features**
   - Settings → Network → Network settings
   - Ensure console is connected to network
   - Note the IP address for manual configuration

2. **Enable Remote Features**
   - Settings → Devices & connections → Remote features
   - Enable "Enable remote features"
   - Check "Enable from any device" for easier setup

3. **Power Settings** (for remote power on)
   - Settings → General → Power mode & startup
   - Set to "Instant-on" mode
   - Enable "Wake on network"

### Network Setup
- Console and Raspberry Pi must be on same network
- No firewall blocking between devices
- Static IP recommended for consoles

### Ports
- **Port 5050**: Primary control port (TCP/UDP)
- Both TCP and UDP protocols are required

### Live ID Requirement
- Xbox Live ID needed for power-on functionality
- Found in Settings → System → Console info
- Stored as AuthToken in device configuration

## Discovery Process

### Automatic Discovery

1. Navigate to **Devices** → **Add Device**
2. Select **Xbox** as connection type
3. Click **Discover Xbox Consoles**
4. Wait for broadcast discovery (5-10 seconds)
5. Select your console from discovered list

The discovery process:
- Broadcasts UDP discovery message to network
- Listens for Xbox console responses
- Parses device information including name and type
- Automatically identifies console generation

### Manual Configuration

If automatic discovery fails:

```json
{
  "name": "Living Room Xbox",
  "type": "GameConsole",
  "connectionType": "Network",
  "ipAddress": "192.168.1.80",
  "port": 5050,
  "model": "Xbox Series X",
  "authToken": "FD00000000000000"
}
```

### Obtaining Xbox Live ID

1. On your Xbox console, go to:
   - Settings → System → Console info
2. Note the "Xbox Live device ID"
3. Enter this as the AuthToken when configuring the device

## Configuration

### Discovery Settings

```json
{
  "Xbox": {
    "Discovery": {
      "Timeout": 10,
      "BroadcastPort": 5050,
      "EnableBroadcast": true
    }
  }
}
```

### Connection Settings

```json
{
  "Xbox": {
    "Connection": {
      "CommandPort": 5050,
      "CommandTimeout": 5000,
      "KeepAlive": true,
      "MaxConcurrentDevices": 10
    }
  }
}
```

## Available Commands

### Navigation Controls

| Command | Xbox Button | Description |
|---------|-------------|-------------|
| `up` | D-Pad Up | Navigate up in menus |
| `down` | D-Pad Down | Navigate down in menus |
| `left` | D-Pad Left | Navigate left in menus |
| `right` | D-Pad Right | Navigate right in menus |

### Action Buttons

| Command | Xbox Button | Description |
|---------|-------------|-------------|
| `a` | A | Select/Confirm |
| `b` | B | Back/Cancel |
| `x` | X | Context action |
| `y` | Y | Menu/Options |

### System Buttons

| Command | Xbox Button | Description |
|---------|-------------|-------------|
| `nexus` | Xbox/Guide | Xbox home menu |
| `menu` | Menu | App menu (≡ button) |
| `view` | View | View button (▭▭ button) |
| `back` | Back | Go back |

### Shoulder/Trigger Buttons

| Command | Xbox Button | Description |
|---------|-------------|-------------|
| `left_shoulder` | LB | Left bumper |
| `right_shoulder` | RB | Right bumper |
| `left_trigger` | LT | Left trigger |
| `right_trigger` | RT | Right trigger |

### Stick Buttons

| Command | Xbox Button | Description |
|---------|-------------|-------------|
| `left_thumbstick` | LS | Left stick click |
| `right_thumbstick` | RS | Right stick click |

### Media Controls

| Command | Description |
|---------|-------------|
| `play_pause` | Toggle play/pause |
| `stop` | Stop playback |
| `fast_forward` | Fast forward |
| `rewind` | Rewind |

### Power Control

| Command | Description |
|---------|-------------|
| `power` | Toggle power state |
| `power_on` | Turn on console (requires Live ID) |
| `power_off` | Turn off/sleep console |

### Number Keys

Direct number input (0-9) is supported for PIN entry and channel selection.

## Button Mapping

### Zapper Commands to Xbox Buttons

```json
{
  "mapping": {
    "Power": "power",
    "Menu": "nexus",
    "Back": "back",
    "DirectionalUp": "up",
    "DirectionalDown": "down", 
    "DirectionalLeft": "left",
    "DirectionalRight": "right",
    "Ok": "a",
    "PlayPause": "play_pause",
    "Stop": "stop",
    "FastForward": "fast_forward",
    "Rewind": "rewind"
  }
}
```

### Custom Button Mapping

```json
{
  "customMapping": {
    "lb": "left_shoulder",
    "rb": "right_shoulder",
    "lt": "left_trigger",
    "rt": "right_trigger",
    "xbox": "nexus",
    "guide": "nexus"
  }
}
```

## Protocol Details

### Communication Protocol

#### TCP Commands
Used for button presses, navigation, and text:
```json
{
  "type": "button",
  "button": "a"
}
```

#### UDP Commands
Used for discovery and power control:
```json
{
  "type": "discovery",
  "version": 2
}
```

#### Text Input
```json
{
  "type": "text",
  "text": "Search query"
}
```

### Discovery Protocol
1. Broadcast UDP discovery packet
2. Xbox responds with device information
3. Parse response for console details
4. Establish TCP connection for control

## Advanced Features

### Text Input

Send text to Xbox for searches, login, and messages:

```csharp
await controller.SendText(device, "Hello Xbox", cancellationToken);
```

Useful for:
- Entering search queries
- Login credentials
- Messages in games/apps
- URL entry in Edge browser

### Power Management

Intelligent power state management:
1. Test connection with ping
2. If console responds: send power off
3. If no response: send power on with Live ID

Power on requires:
- Console in Instant-on mode
- Valid Xbox Live ID
- Network wake enabled

### Connection Persistence

- Maintains connection pool for active consoles
- Reduces latency for subsequent commands
- Automatic cleanup of stale connections
- Thread-safe concurrent access

### Custom Commands

Support for game-specific controls:

```json
{
  "customCommands": {
    "screenshot": {
      "button": "xbox",
      "hold": true,
      "duration": 500
    }
  }
}
```

## Troubleshooting

### Discovery Issues

#### No Consoles Found
1. **Verify Network Settings**
   - Console connected to same network
   - Remote features enabled
   ```bash
   ping [XBOX-IP]
   ```

2. **Check Firewall**
   - Allow UDP port 5050
   - Disable network isolation

3. **Manual Test**
   ```bash
   nc -u [XBOX-IP] 5050
   ```

#### Console Not Responding
- Wake console manually first
- Check if another device is connected
- Verify remote features enabled

### Connection Problems

#### Commands Not Working
1. **Verify Remote Features**
   - Settings → Devices & connections → Remote features
   - Must be enabled

2. **Test Connection**
   ```bash
   telnet [XBOX-IP] 5050
   ```

3. **Check Console State**
   - Console not in game that blocks remote
   - No system update in progress

### Power Control Issues

#### Console Won't Turn On
1. **Verify Instant-on Mode**
   - Not available in Energy-saving mode
   - Settings → Power mode & startup

2. **Check Live ID**
   - Correct Xbox Live device ID
   - Properly stored as AuthToken

3. **Network Wake Settings**
   - "Wake on network" must be enabled

#### Console Won't Turn Off
- Some games prevent sleep
- Try navigating to home first
- May need manual intervention

## API Usage

### Discover Xbox Consoles

```http
POST /api/devices/discover/xbox
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
      "name": "Xbox-SystemOS",
      "ipAddress": "192.168.1.80",
      "liveId": "FD00000000000000",
      "consoleType": "XboxSeriesX",
      "isAuthenticated": false
    }
  ]
}
```

### Send Command

```http
POST /api/devices/{deviceId}/commands
Content-Type: application/json

{
  "command": "a"
}
```

### Send Custom Command

```http
POST /api/devices/{deviceId}/commands
Content-Type: application/json

{
  "command": "custom",
  "networkPayload": "menu"
}
```

### Send Text

```http
POST /api/devices/{deviceId}/text
Content-Type: application/json

{
  "text": "Halo Infinite"
}
```

### Test Connection

```http
GET /api/devices/{deviceId}/test
```

## Security Considerations

### Network Security
- Commands sent unencrypted
- Isolate gaming devices on separate VLAN
- Use firewall rules to restrict access

### Live ID Storage
- Store Live ID encrypted
- Limit access to authorized users
- Don't expose in logs or UI

### Access Control
- Implement authentication for API
- Rate limit commands
- Monitor for unusual activity

## Integration Tips

### Performance Optimization
1. **Static IP Configuration**
   - Assign static IPs to consoles
   - Faster connection establishment

2. **Connection Pooling**
   - Reuse existing connections
   - Reduce command latency

3. **Batch Commands**
   - Group related commands
   - Minimize network overhead

### Activity Examples

#### Gaming Session
```json
{
  "name": "Xbox Gaming",
  "commands": [
    { "device": "xboxSeriesX", "command": "power_on" },
    { "delay": 10000 },
    { "device": "tv", "command": "hdmi1" },
    { "device": "tv", "command": "game_mode_on" },
    { "device": "soundbar", "command": "input_hdmi" }
  ]
}
```

#### Media Streaming
```json
{
  "name": "Netflix on Xbox",
  "commands": [
    { "device": "xboxOne", "command": "power_on" },
    { "delay": 10000 },
    { "device": "xboxOne", "command": "nexus" },
    { "delay": 2000 },
    { "device": "tv", "command": "hdmi1" }
  ]
}
```

#### Quick Screenshot
```json
{
  "name": "Screenshot",
  "commands": [
    { "device": "xbox", "command": "custom", "payload": "xbox", "hold": 500 }
  ]
}
```

## Known Limitations

### Current Implementation
- Cannot launch specific games/apps
- No party chat control
- No achievement notifications
- No screenshot/clip management
- No store navigation
- No profile switching
- Limited to basic remote control
- No controller vibration feedback

### Protocol Limitations
- Unofficial protocol (reverse engineered)
- No encryption or authentication
- Limited to local network
- Some features require console UI

### Console Limitations
- Instant-on required for remote power
- Some games block remote input
- System updates disable remote features
- One active remote connection at a time

### Future Enhancements
- SmartGlass protocol integration
- Game/app launching
- Enhanced media control
- Party and chat management
- Cloud gaming integration
- Voice command support