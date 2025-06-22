# Configuration Guide

This guide explains how to configure ZapperHub devices, activities, and system settings for optimal performance.

## Overview

ZapperHub configuration involves:
- **Device Setup**: Adding and configuring controllable devices
- **Activity Creation**: Setting up one-touch automation sequences
- **System Configuration**: Adjusting hardware and software settings
- **IR Code Management**: Setting up infrared control codes

## Device Configuration

### Adding Devices

#### Via Web Interface

1. **Navigate to Devices Tab**
2. **Click "Add Device" Button**
3. **Fill Device Information:**
   - **Name**: Descriptive name (e.g., "Living Room TV")
   - **Brand**: Manufacturer name (Samsung, LG, Sony, etc.)
   - **Model**: Device model number
   - **Type**: Device category (TV, Receiver, etc.)
   - **Connection Type**: How ZapperHub communicates with device

#### Device Types

| Type | Description | Common Brands |
|------|-------------|---------------|
| Television | TVs and displays | Samsung, LG, Sony, TCL |
| Receiver | Audio/video receivers | Denon, Yamaha, Onkyo |
| Cable Box | Cable/satellite boxes | Xfinity, DirecTV, Dish |
| Streaming Device | Media streamers | Roku, Apple TV, Fire TV |
| Game Console | Gaming systems | Xbox, PlayStation, Switch |
| Sound Bar | Audio systems | Sonos, Bose, Samsung |
| Blu-ray Player | Disc players | Sony, Panasonic, LG |
| Smart TV | Internet-connected TVs | LG WebOS, Samsung Tizen |

#### Connection Types

##### Infrared (IR)
**Best for**: Traditional TVs, receivers, cable boxes

```json
{
  "name": "Living Room TV",
  "brand": "Samsung",
  "model": "QN65Q80A",
  "type": "Television",
  "connectionType": "InfraredIR"
}
```

**Requirements:**
- IR LED circuit on GPIO 18
- Line of sight to device
- Compatible IR codes in database

##### WebOS (LG Smart TVs)
**Best for**: LG smart TVs (2014+)

```json
{
  "name": "Master Bedroom TV",
  "brand": "LG",
  "model": "OLED55C1",
  "type": "SmartTV",
  "connectionType": "WebOS",
  "ipAddress": "192.168.1.100"
}
```

**Setup Process:**
1. Enable "LG Connect Apps" on TV
2. Connect TV to same network as ZapperHub
3. Use auto-discovery or enter IP manually
4. Accept pairing request on TV

##### Bluetooth
**Best for**: Apple TV, Android TV, game controllers

```json
{
  "name": "Apple TV",
  "brand": "Apple",
  "model": "Apple TV 4K",
  "type": "StreamingDevice",
  "connectionType": "Bluetooth",
  "macAddress": "AA:BB:CC:DD:EE:FF"
}
```

**Setup Process:**
1. Put device in pairing mode
2. Use ZapperHub discovery
3. Complete pairing process
4. Test basic commands

##### Network TCP/WebSocket
**Best for**: IP-controlled receivers, smart devices

```json
{
  "name": "Denon Receiver",
  "brand": "Denon",
  "model": "AVR-X3700H",
  "type": "Receiver",
  "connectionType": "NetworkTCP",
  "ipAddress": "192.168.1.110",
  "port": 23
}
```

### Device Discovery

#### Automatic Discovery

**WebOS TVs:**
```bash
# From web interface: Devices → Discover → WebOS
# Scans local network for LG TVs
# Automatically configures connection settings
```

**Bluetooth Devices:**
```bash
# From web interface: Devices → Discover → Bluetooth
# Scans for nearby Bluetooth devices
# Shows available devices for pairing
```

#### Manual Configuration

When automatic discovery fails:

1. **Find Device IP Address**
   - Check router admin panel
   - Use network scanner: `nmap -sn 192.168.1.0/24`
   - Check device network settings

2. **Determine Connection Method**
   - IR: Traditional remote control
   - Network: Ethernet/WiFi connected
   - Bluetooth: Wireless personal area network

3. **Test Connectivity**
   - Ping device: `ping [device-ip]`
   - Check open ports: `nmap [device-ip]`
   - Verify protocols supported

### IR Code Configuration

#### Using Built-in Database

ZapperHub includes IR codes for popular devices:

1. **Select Brand and Model** during device setup
2. **Codes are automatically assigned** from database
3. **Test commands** to verify compatibility

#### Adding Custom IR Codes

**Method 1: Manual Entry**
```json
{
  "commandName": "Power",
  "protocol": "NEC",
  "hexCode": "0xE0E040BF",
  "frequency": 38000
}
```

**Method 2: Raw Pulse Data**
```json
{
  "commandName": "VolumeUp",
  "protocol": "RAW",
  "rawData": "9000 4500 560 560 560 1690 560 560 560 1690",
  "frequency": 38000
}
```

**Method 3: Import Code Set**
```bash
# Create JSON file with IR codes
curl -X POST http://localhost:5000/api/ir-codes/sets \
  -H "Content-Type: application/json" \
  -d @custom-codes.json
```

## Activity Configuration

### Creating Activities

Activities automate multiple device commands in sequence:

#### Example: "Watch TV" Activity

```json
{
  "name": "Watch TV",
  "description": "Turn on TV and receiver, switch to cable",
  "isEnabled": true,
  "steps": [
    {
      "deviceId": 1,
      "commandName": "Power",
      "delayMs": 2000,
      "stepOrder": 1
    },
    {
      "deviceId": 2,
      "commandName": "Power",
      "delayMs": 3000,
      "stepOrder": 2
    },
    {
      "deviceId": 2,
      "commandName": "Input_HDMI1",
      "delayMs": 1000,
      "stepOrder": 3
    }
  ]
}
```

#### Activity Design Best Practices

**Timing Considerations:**
- Allow 2-3 seconds for device power-up
- Add delays between input switches
- Consider warm-up times for projectors

**Command Ordering:**
1. **Power on devices** (slowest to fastest)
2. **Wait for initialization**
3. **Set inputs and modes**
4. **Adjust volume/settings**

**Error Handling:**
- Test activities thoroughly
- Have backup manual controls
- Consider device state verification

#### Common Activity Templates

**Movie Night:**
```json
{
  "name": "Movie Night",
  "steps": [
    {"device": "Receiver", "command": "Power", "delay": 3000},
    {"device": "TV", "command": "Power", "delay": 2000},
    {"device": "Blu-ray", "command": "Power", "delay": 2000},
    {"device": "Receiver", "command": "Input_BluRay", "delay": 1000},
    {"device": "TV", "command": "Input_HDMI_ARC", "delay": 1000},
    {"device": "Receiver", "command": "SurroundMode_Movie", "delay": 500}
  ]
}
```

**All Off:**
```json
{
  "name": "All Off",
  "steps": [
    {"device": "TV", "command": "Power", "delay": 1000},
    {"device": "Receiver", "command": "Power", "delay": 1000},
    {"device": "Cable Box", "command": "Power", "delay": 1000},
    {"device": "Blu-ray", "command": "Power", "delay": 1000}
  ]
}
```

### Activity Troubleshooting

**Activity Doesn't Complete:**
- Check device power states
- Increase delays between commands
- Verify IR line of sight
- Test individual commands

**Devices Don't Respond:**
- Verify device configurations
- Check connection status
- Test manual control first
- Review activity logs

## System Configuration

### Application Settings

#### appsettings.json Configuration

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=zapper.db"
  },
  "Hardware": {
    "IRTransmitter": {
      "GpioPin": 18,
      "Enabled": true,
      "PowerLevel": "Medium"
    },
    "Bluetooth": {
      "Enabled": true,
      "ScanTimeout": 30,
      "AutoConnect": true
    },
    "WebOS": {
      "DiscoveryTimeout": 10,
      "AutoPair": false
    }
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "ZapperHub": "Debug",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

#### Environment Variables

```bash
# Database location
export ConnectionStrings__DefaultConnection="Data Source=/data/zapper.db"

# Hardware configuration
export Hardware__IRTransmitter__GpioPin=18
export Hardware__IRTransmitter__Enabled=true

# Logging level
export Logging__LogLevel__Default=Information

# Network binding
export ASPNETCORE_URLS="http://0.0.0.0:5000"
```

### Hardware Configuration

#### GPIO Pin Assignment

```bash
# Default IR transmitter pin
GPIO_IR_PIN=18

# Optional IR receiver pin
GPIO_IR_RECEIVE_PIN=19

# Status LED pin (optional)
GPIO_STATUS_LED=21
```

#### IR Transmitter Settings

**Power Levels:**
- **Low**: Single IR LED, basic range
- **Medium**: Single LED with amplifier
- **High**: Multiple LEDs, extended range

**Frequency Settings:**
- **38kHz**: Most common (default)
- **40kHz**: Sony devices
- **36kHz**: Some older devices
- **56kHz**: Newer Samsung devices

#### Bluetooth Configuration

```bash
# Enable Bluetooth service
sudo systemctl enable bluetooth

# Configure Bluetooth settings
sudo nano /etc/bluetooth/main.conf

# Key settings:
# Class = 0x000100
# DiscoverableTimeout = 0
# PairableTimeout = 0
```

### Network Configuration

#### Port Configuration

| Port | Protocol | Purpose |
|------|----------|---------|
| 5000 | HTTP | Web interface and REST API |
| 5001 | HTTPS | Secure web interface (optional) |
| 3000 | TCP | LG WebOS communication |
| Various | UDP | Device discovery protocols |

#### Firewall Rules

```bash
# Allow ZapperHub web interface
sudo ufw allow 5000/tcp

# Allow WebOS discovery
sudo ufw allow 3000/tcp

# Allow Bluetooth
sudo ufw allow 1714:1764/tcp
sudo ufw allow 1714:1764/udp
```

#### Network Optimization

```bash
# Increase network buffer sizes for better performance
echo 'net.core.rmem_max = 16777216' | sudo tee -a /etc/sysctl.conf
echo 'net.core.wmem_max = 16777216' | sudo tee -a /etc/sysctl.conf

# Enable IP forwarding (if needed for routing)
echo 'net.ipv4.ip_forward = 1' | sudo tee -a /etc/sysctl.conf
```

### Performance Tuning

#### Raspberry Pi Optimization

```bash
# Increase GPU memory (if using HDMI output)
echo 'gpu_mem=128' | sudo tee -a /boot/config.txt

# Disable unnecessary services
sudo systemctl disable triggerhappy
sudo systemctl disable avahi-daemon

# Enable hardware random number generator
echo 'dtparam=random=on' | sudo tee -a /boot/config.txt
```

#### Application Performance

```json
{
  "Kestrel": {
    "Limits": {
      "MaxConcurrentConnections": 100,
      "MaxRequestBodySize": 1048576
    }
  },
  "SignalR": {
    "KeepAliveInterval": "00:00:15",
    "ClientTimeoutInterval": "00:00:30"
  }
}
```

## Advanced Configuration

### Custom Device Protocols

#### Adding New Device Types

```csharp
// Add to DeviceType enum
public enum DeviceType
{
    // ... existing types
    SmartSpeaker = 9,
    SecurityCamera = 10,
    SmartLight = 11
}
```

#### Custom Command Handlers

```csharp
public class CustomDeviceController : IDeviceController
{
    public async Task<bool> SendCommandAsync(Device device, DeviceCommand command, CancellationToken cancellationToken)
    {
        // Custom protocol implementation
        return true;
    }
}
```

### Integration Configuration

#### Home Assistant Integration

```yaml
# configuration.yaml
rest_command:
  zapperhub_activity:
    url: "http://192.168.1.100:5000/api/activities/{{ activity_id }}/execute"
    method: POST

automation:
  - alias: "Evening Movie Mode"
    trigger:
      platform: time
      at: "20:00:00"
    condition:
      condition: state
      entity_id: input_boolean.movie_night
      state: 'on'
    action:
      service: rest_command.zapperhub_activity
      data:
        activity_id: 1
```

#### Node-RED Integration

```json
[
    {
        "id": "zapperhub-flow",
        "type": "function",
        "func": "msg.url = 'http://192.168.1.100:5000/api/devices/' + msg.deviceId + '/command';\nmsg.method = 'POST';\nmsg.headers = {'Content-Type': 'application/json'};\nmsg.payload = {commandName: msg.command};\nreturn msg;"
    }
]
```

### Backup and Recovery

#### Configuration Backup

```bash
#!/bin/bash
# backup-config.sh

BACKUP_DIR="/opt/zapperhub/backups/$(date +%Y%m%d_%H%M%S)"
mkdir -p "$BACKUP_DIR"

# Backup database
cp /opt/zapperhub/publish/zapper.db "$BACKUP_DIR/"

# Backup configuration
cp /opt/zapperhub/publish/appsettings*.json "$BACKUP_DIR/"

# Backup custom IR codes
curl http://localhost:5000/api/ir-codes/sets > "$BACKUP_DIR/ir-codes.json"

echo "Backup completed: $BACKUP_DIR"
```

#### Automated Backups

```bash
# Add to crontab (crontab -e)
0 2 * * * /opt/zapperhub/backup-config.sh
```

## Security Configuration

### Basic Security

```bash
# Change default SSH password
passwd

# Disable SSH password authentication (use keys)
sudo nano /etc/ssh/sshd_config
# Set: PasswordAuthentication no

# Enable UFW firewall
sudo ufw enable

# Regular updates
sudo apt update && sudo apt upgrade -y
```

### Network Security

```bash
# Restrict access to local network only
sudo ufw deny from any to any port 5000
sudo ufw allow from 192.168.1.0/24 to any port 5000

# Use strong WiFi security (WPA3)
# Consider VPN for remote access
```

---

**Proper configuration ensures optimal ZapperHub performance and reliability.** ⚙️