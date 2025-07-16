# Bluetooth Integration

## Overview

Bluetooth integration enables Zapper to control modern streaming devices like Apple TV, Android TV, Fire TV, and other Bluetooth-enabled devices. Using Bluetooth HID (Human Interface Device) profiles, Zapper can send remote control commands with reliable, bidirectional communication.

## Supported Devices

### Streaming Devices
- **Apple TV** (4th generation and newer)
- **Android TV** devices (Shield TV, Chromecast with Google TV, etc.)
- **Amazon Fire TV** (Stick 4K and newer)
- **Roku** devices with Bluetooth remotes

### Smart TVs
- **Sony Android TVs**
- **TCL Google TV**
- **Hisense Android TV**
- **Philips Android TV**

### Audio Devices
- **Bluetooth soundbars**
- **Bluetooth AV receivers**
- **Bluetooth speakers** (limited control)

## Hardware Requirements

### Raspberry Pi Built-in Bluetooth
- Raspberry Pi 3B+ or newer (built-in Bluetooth 4.2/5.0)
- No additional hardware required

### External Bluetooth Adapter (Optional)
- For better range or older Pi models
- USB Bluetooth 5.0 adapter recommended
- Must support HID profile

## Configuration

### Enable Bluetooth in Configuration

```json
{
  "Hardware": {
    "Bluetooth": {
      "Enabled": true,
      "AdapterName": "hci0",
      "ScanTimeout": 30,
      "ConnectionTimeout": 10,
      "AutoReconnect": true
    }
  }
}
```

### System Requirements

Ensure Bluetooth service is running:
```bash
sudo systemctl enable bluetooth
sudo systemctl start bluetooth
```

## Pairing Process

### Automatic Discovery

1. Navigate to **Devices** → **Add Device**
2. Select **Bluetooth** as connection type
3. Click **Scan for Devices**
4. Put target device in pairing mode
5. Select device from discovered list
6. Complete pairing process

### Manual Pairing

For devices that don't appear in automatic discovery:

1. Get device MAC address:
   ```bash
   sudo bluetoothctl
   scan on
   # Note the MAC address of your device
   ```

2. Add device manually in Zapper:
   ```json
   {
     "name": "Apple TV",
     "type": "StreamingDevice",
     "connectionType": "Bluetooth",
     "bluetoothAddress": "AA:BB:CC:DD:EE:FF",
     "profile": "AppleTV"
   }
   ```

## Device Profiles

### Apple TV Profile

```json
{
  "profile": "AppleTV",
  "hidDescriptor": "0x05010906a101050719e029e71500250175019508810295017508810195067508150026ff000509190129658102950175088103950575010508190129059102950175039103950675081500256505090738192938150025658100c0",
  "commands": {
    "up": [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0B, 0x00],
    "down": [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0D, 0x00],
    "left": [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0E, 0x00],
    "right": [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0F, 0x00],
    "select": [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x5D, 0x00],
    "menu": [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x86, 0x00],
    "playPause": [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x5E, 0x00],
    "home": [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x60, 0x00]
  }
}
```

### Android TV Profile

```json
{
  "profile": "AndroidTV",
  "hidDescriptor": "0x05010906a1018501050719e029e71500250175019508810295017508810395057501050819012905910295017503910395067508150025650509190129658102950175088103950575010508190129059102950175039103950675080508190129059102c0",
  "commands": {
    "up": [0xA1, 0x01, 0x00, 0x00, 0x00, 0x52, 0x00, 0x00],
    "down": [0xA1, 0x01, 0x00, 0x00, 0x00, 0x51, 0x00, 0x00],
    "left": [0xA1, 0x01, 0x00, 0x00, 0x00, 0x50, 0x00, 0x00],
    "right": [0xA1, 0x01, 0x00, 0x00, 0x00, 0x4F, 0x00, 0x00],
    "select": [0xA1, 0x01, 0x00, 0x00, 0x00, 0x28, 0x00, 0x00],
    "back": [0xA1, 0x01, 0x00, 0x00, 0x00, 0x29, 0x00, 0x00],
    "home": [0xA1, 0x01, 0x00, 0x00, 0x00, 0x4A, 0x00, 0x00],
    "volumeUp": [0xA1, 0x01, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00],
    "volumeDown": [0xA1, 0x01, 0x00, 0x00, 0x00, 0x81, 0x00, 0x00]
  }
}
```

## Advanced Features

### Auto-Reconnection

Zapper automatically reconnects to paired devices when they become available:

```json
{
  "bluetoothSettings": {
    "autoReconnect": true,
    "reconnectInterval": 5000,
    "maxReconnectAttempts": 3
  }
}
```

### Multiple Device Support

Control multiple Bluetooth devices simultaneously:
- Each device maintains its own connection
- Commands are routed to the correct device
- Connection status tracked independently

### Custom HID Commands

For unsupported devices, define custom HID commands:

```json
{
  "customCommands": {
    "customButton1": {
      "report": [0xA1, 0x01, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00],
      "description": "Custom function button"
    }
  }
}
```

## Troubleshooting

### Device Not Found During Scan

1. **Ensure device is in pairing mode**
   - Apple TV: Settings → Remotes and Devices → Bluetooth
   - Android TV: Settings → Remote & Accessories → Add accessory

2. **Check Bluetooth service status**
   ```bash
   sudo systemctl status bluetooth
   ```

3. **Reset Bluetooth adapter**
   ```bash
   sudo systemctl restart bluetooth
   ```

### Connection Drops Frequently

1. **Check signal strength**
   - Move Raspberry Pi closer to device
   - Remove obstacles between devices

2. **Disable power saving**
   ```bash
   sudo btmgmt power on
   ```

3. **Update Bluetooth firmware**
   ```bash
   sudo apt update
   sudo apt install bluez
   ```

### Commands Not Working

1. **Verify correct profile selected**
2. **Check HID descriptor matches device**
3. **Monitor Bluetooth logs**
   ```bash
   sudo journalctl -u bluetooth -f
   ```

## API Usage

### List Paired Devices

```http
GET /api/bluetooth/devices
```

### Scan for New Devices

```http
POST /api/bluetooth/scan
Content-Type: application/json

{
  "timeout": 30
}
```

### Send Bluetooth Command

```http
POST /api/devices/{deviceId}/commands
Content-Type: application/json

{
  "command": "select",
  "holdDuration": 0
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
  "signalStrength": -45,
  "battery": 85,
  "lastSeen": "2024-01-15T10:30:00Z"
}
```

## Security Considerations

### Pairing Security
- Always use secure pairing methods
- Don't leave devices in pairing mode unnecessarily
- Remove unused paired devices

### Access Control
- Limit Bluetooth access to Zapper service
- Use authentication for API endpoints
- Monitor for unauthorized connections

## Performance Tips

### Optimize Connection Stability
1. Use external Bluetooth adapter for better range
2. Position Pi centrally to all devices
3. Minimize interference from Wi-Fi (use 5GHz)
4. Keep firmware updated

### Reduce Latency
1. Disable unnecessary Bluetooth services
2. Limit concurrent connections
3. Use wired Ethernet for Pi network
4. Optimize HID report size