# API Reference

Zapper provides a comprehensive RESTful API for device control, automation, and integration with other systems.

## Base URL

```
http://[raspberry-pi-ip]:5000/api
```

## Authentication

Currently, Zapper operates without authentication. For production deployments, consider implementing:
- API keys
- OAuth 2.0
- JWT tokens
- Network-level security (firewall, VPN)

## Response Format

All API responses follow a consistent JSON format:

```json
{
  "data": { ... },
  "success": true,
  "message": "Operation completed successfully",
  "timestamp": "2025-01-20T10:30:00Z"
}
```

Error responses:
```json
{
  "error": "Error description",
  "success": false,
  "timestamp": "2025-01-20T10:30:00Z"
}
```

## Devices API

### Get All Devices

```http
GET /api/devices
```

**Response:**
```json
[
  {
    "id": 1,
    "name": "Living Room TV",
    "brand": "Samsung",
    "model": "QN65Q80A",
    "type": 0,
    "connectionType": 0,
    "ipAddress": null,
    "port": null,
    "macAddress": null,
    "isOnline": true,
    "lastSeen": "2025-01-20T10:30:00Z"
  }
]
```

### Get Device by ID

```http
GET /api/devices/{id}
```

**Parameters:**
- `id` (integer): Device identifier

### Create Device

```http
POST /api/devices
```

**Request Body:**
```json
{
  "name": "Bedroom TV",
  "brand": "LG",
  "model": "OLED55C1",
  "type": 0,
  "connectionType": 6,
  "ipAddress": "192.168.1.100",
  "port": 3000
}
```

### Update Device

```http
PUT /api/devices/{id}
```

### Delete Device

```http
DELETE /api/devices/{id}
```

### Send Command to Device

```http
POST /api/devices/{id}/command
```

**Request Body:**
```json
{
  "commandName": "Power",
  "parameters": {}
}
```

### Test Device Connection

```http
POST /api/devices/{id}/test
```

## Device Discovery API

### Discover WebOS TVs

```http
POST /api/devices/discover/webos
```

**Request Body:**
```json
{
  "timeoutSeconds": 10
}
```

**Response:**
```json
[
  {
    "name": "LG TV",
    "ipAddress": "192.168.1.100",
    "macAddress": "AA:BB:CC:DD:EE:FF",
    "modelName": "OLED55C1",
    "firmwareVersion": "03.33.85"
  }
]
```

### Discover Bluetooth Devices

```http
GET /api/devices/discover/bluetooth
```

## Activities API

### Get All Activities

```http
GET /api/activities
```

### Get Activity by ID

```http
GET /api/activities/{id}
```

### Create Activity

```http
POST /api/activities
```

**Request Body:**
```json
{
  "name": "Watch TV",
  "description": "Turn on TV and receiver",
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
      "delayMs": 1000,
      "stepOrder": 2
    }
  ]
}
```

### Execute Activity

```http
POST /api/activities/{id}/execute
```

### Update Activity

```http
PUT /api/activities/{id}
```

### Delete Activity

```http
DELETE /api/activities/{id}
```

## IR Codes API

### Get IR Code Sets

```http
GET /api/ir-codes/sets
```

### Search IR Code Sets

```http
GET /api/ir-codes/sets/search?brand=Samsung&deviceType=0
```

**Query Parameters:**
- `brand` (string): Device brand
- `model` (string): Device model
- `deviceType` (integer): Device type enum

### Get IR Code Set

```http
GET /api/ir-codes/sets/{id}
```

### Get IR Codes for Set

```http
GET /api/ir-codes/sets/{codeSetId}/codes
```

### Get Specific IR Code

```http
GET /api/ir-codes/sets/{codeSetId}/codes/{commandName}
```

### Create IR Code Set

```http
POST /api/ir-codes/sets
```

**Request Body:**
```json
{
  "brand": "Sony",
  "model": "XBR-55X90J",
  "deviceType": 0,
  "description": "Sony TV IR codes",
  "codes": [
    {
      "commandName": "Power",
      "protocol": "SONY",
      "hexCode": "0xA90",
      "frequency": 40000
    }
  ]
}
```

### Add IR Code to Set

```http
POST /api/ir-codes/sets/{codeSetId}/codes
```

### Export IR Code Set

```http
GET /api/ir-codes/sets/{id}/export
```

Returns JSON file download.

### Seed Default IR Codes

```http
POST /api/ir-codes/seed
```

## System API

### Get System Status

```http
GET /api/system/status
```

**Response:**
```json
{
  "status": "Running",
  "uptime": "2 days, 14:32:17",
  "version": "1.0.0",
  "connectedClients": 3,
  "system": {
    "platform": "Linux ARM64",
    "dotnetVersion": "9.0.0",
    "totalMemory": "4GB",
    "usedMemory": "512MB",
    "cpuUsage": "15%"
  },
  "hardware": {
    "irTransmitterEnabled": true,
    "bluetoothEnabled": true,
    "gpioAvailable": true
  }
}
```

### Get System Logs

```http
GET /api/system/logs?level=Info&count=100
```

**Query Parameters:**
- `level` (string): Log level filter
- `count` (integer): Number of log entries
- `since` (datetime): Logs since timestamp

## WebSocket/SignalR API

### Connection

```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/zapper")
    .build();

await connection.start();
```

### Events

#### Device Status Changed
```javascript
connection.on("DeviceStatusChanged", (data) => {
    console.log(`Device ${data.DeviceName} is ${data.IsOnline ? 'online' : 'offline'}`);
});
```

#### Device Command Executed
```javascript
connection.on("DeviceCommandExecuted", (data) => {
    console.log(`Command ${data.CommandName} on ${data.DeviceName}: ${data.Success}`);
});
```

#### Activity Status Changed
```javascript
connection.on("ActivityStatusChanged", (data) => {
    console.log(`Activity ${data.ActivityName} status: ${data.Status}`);
});
```

#### System Message
```javascript
connection.on("SystemMessage", (data) => {
    console.log(`System: ${data.Message} (${data.Level})`);
});
```

## Data Models

### Device Types

```csharp
public enum DeviceType
{
    Television = 0,
    Receiver = 1,
    CableBox = 2,
    StreamingDevice = 3,
    GameConsole = 4,
    SoundBar = 5,
    BlurayPlayer = 6,
    MediaPlayer = 7,
    SmartTV = 8
}
```

### Connection Types

```csharp
public enum ConnectionType
{
    InfraredIR = 0,
    RadioFrequencyRF = 1,
    NetworkTCP = 2,
    NetworkWebSocket = 3,
    Bluetooth = 4,
    USB = 5,
    WebOS = 6
}
```

### Command Types

```csharp
public enum CommandType
{
    Power = 0,
    VolumeUp = 1,
    VolumeDown = 2,
    Mute = 3,
    ChannelUp = 4,
    ChannelDown = 5,
    DirectionalUp = 6,
    DirectionalDown = 7,
    DirectionalLeft = 8,
    DirectionalRight = 9,
    OK = 10,
    Back = 11,
    Home = 12,
    Menu = 13,
    PlayPause = 14,
    Stop = 15,
    FastForward = 16,
    Rewind = 17
}
```

## Error Codes

| Code | Description | Resolution |
|------|-------------|------------|
| 400 | Bad Request | Check request format and parameters |
| 404 | Not Found | Verify device/activity exists |
| 500 | Internal Server Error | Check server logs |
| 503 | Service Unavailable | Check hardware connections |

## Rate Limiting

Current implementation has no rate limiting. For production:
- Consider implementing rate limiting middleware
- Recommended: 100 requests per minute per IP
- Activity execution: 10 per minute per device

## SDK and Client Libraries

### JavaScript/TypeScript

```javascript
class ZapperClient {
    constructor(baseUrl) {
        this.baseUrl = baseUrl;
    }
    
    async getDevices() {
        const response = await fetch(`${this.baseUrl}/api/devices`);
        return await response.json();
    }
    
    async sendCommand(deviceId, command) {
        const response = await fetch(`${this.baseUrl}/api/devices/${deviceId}/command`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ commandName: command })
        });
        return await response.json();
    }
}
```

### Python

```python
import requests
import json

class ZapperClient:
    def __init__(self, base_url):
        self.base_url = base_url
    
    def get_devices(self):
        response = requests.get(f"{self.base_url}/api/devices")
        return response.json()
    
    def send_command(self, device_id, command):
        data = {"commandName": command}
        response = requests.post(
            f"{self.base_url}/api/devices/{device_id}/command",
            json=data
        )
        return response.json()
```

### C#

```csharp
public class ZapperClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    
    public ZapperClient(string baseUrl)
    {
        _baseUrl = baseUrl;
        _httpClient = new HttpClient();
    }
    
    public async Task<List<Device>> GetDevicesAsync()
    {
        var response = await _httpClient.GetStringAsync($"{_baseUrl}/api/devices");
        return JsonSerializer.Deserialize<List<Device>>(response);
    }
    
    public async Task SendCommandAsync(int deviceId, string command)
    {
        var data = new { commandName = command };
        var json = JsonSerializer.Serialize(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        await _httpClient.PostAsync($"{_baseUrl}/api/devices/{deviceId}/command", content);
    }
}
```

## Integration Examples

### Home Assistant

```yaml
# configuration.yaml
rest_command:
  zapper_tv_power:
    url: "http://192.168.1.100:5000/api/devices/1/command"
    method: POST
    headers:
      Content-Type: "application/json"
    payload: '{"commandName": "Power"}'

automation:
  - alias: "Turn on TV when motion detected"
    trigger:
      platform: state
      entity_id: binary_sensor.living_room_motion
      to: 'on'
    action:
      service: rest_command.zapper_tv_power
```

### Node-RED

```json
[
    {
        "id": "zapper-node",
        "type": "http request",
        "method": "POST",
        "url": "http://192.168.1.100:5000/api/devices/1/command",
        "headers": {"Content-Type": "application/json"},
        "payload": "{\"commandName\": \"Power\"}"
    }
]
```

### Webhook Integration

```javascript
// Express.js webhook endpoint
app.post('/webhook/zapper', (req, res) => {
    const { device, command } = req.body;
    
    fetch(`http://192.168.1.100:5000/api/devices/${device}/command`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ commandName: command })
    });
    
    res.json({ success: true });
});
```

---

**For more examples and detailed integration guides, see the [Examples](EXAMPLES.md) documentation.** 📚