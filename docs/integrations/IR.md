# Infrared (IR) Integration

## Overview

Infrared control is the most universal integration method, supporting thousands of devices from TVs to cable boxes, receivers, and more. Zapper transmits IR signals through an IR LED connected to the Raspberry Pi's GPIO pins.

## Hardware Requirements

- **IR LED** (940nm wavelength recommended)
- **NPN Transistor** (2N2222, 2N3904, or similar)
- **330Ω Resistor**
- **Jumper wires**

## Circuit Setup

Connect the IR transmitter circuit to GPIO pin 18 (default):

```
Raspberry Pi GPIO 18 ──┬─── 330Ω Resistor ─── Transistor Base
                       │
                       └─── IR LED Anode
                            IR LED Cathode ─── Transistor Emitter ─── Ground
                            
Transistor Collector ─── 3.3V or 5V Power
```

## Configuration

IR transmission is configured in `appsettings.json`:

```json
{
  "Hardware": {
    "IRTransmitter": {
      "GpioPin": 18,
      "Enabled": true,
      "CarrierFrequency": 38000,
      "DutyCycle": 0.33
    }
  }
}
```

## Supported IR Protocols

Zapper supports the following IR protocols:

### 1. NEC Protocol
- **Used by**: Samsung, LG, many Asian brands
- **Carrier**: 38kHz
- **Format**: 32-bit codes

### 2. Sony SIRC
- **Used by**: Sony devices
- **Carrier**: 40kHz
- **Format**: 12, 15, or 20-bit codes

### 3. RC5/RC6
- **Used by**: Philips, some European brands
- **Carrier**: 36kHz
- **Toggle bit**: For repeat detection

### 4. Raw Timing
- **Used for**: Devices with proprietary protocols
- **Format**: Specify exact pulse/space timings

## Adding IR Devices

### Using Predefined Codes

```json
{
  "name": "Samsung TV",
  "type": "Television",
  "connectionType": "Infrared",
  "brand": "Samsung",
  "model": "UN55KU6300",
  "commands": {
    "power": "0xE0E040BF",
    "volumeUp": "0xE0E0E01F",
    "volumeDown": "0xE0E0D02F",
    "channelUp": "0xE0E048B7",
    "channelDown": "0xE0E008F7"
  }
}
```

### Learning IR Codes

1. Navigate to Device Management
2. Select "Learn IR Code"
3. Point original remote at IR receiver
4. Press the button to learn
5. Save the captured code

## IR Code Database

Zapper includes a comprehensive database of IR codes for popular brands:

- **Samsung**: Full command set for TVs (2010-2024 models)
- **LG**: Complete TV control codes
- **Sony**: TV and audio equipment codes
- **Panasonic**: Basic TV functions
- **TCL/Hisense/Vizio**: Common commands

## Troubleshooting IR

### No Response from Device

- Verify IR LED is connected to correct GPIO pin
- Check LED is transmitting (use phone camera to see IR light)
- Ensure correct protocol and carrier frequency
- Try increasing transmission power or repositioning LED

### Intermittent Control

- Check for adequate power supply
- Ensure transistor is properly connected
- Verify IR LED is aimed at device's receiver
- Consider using multiple IR LEDs for better coverage

### Wrong Commands Executed

- Verify correct device brand/model selected
- Check for IR code conflicts with other devices
- Update to latest IR code database

## Advanced IR Features

### Macros
Chain multiple IR commands with configurable delays between commands.

### Discrete Codes
Use discrete on/off codes instead of toggle commands for more reliable control.

### IR Blasting
Control multiple devices in different rooms using IR repeaters or multiple IR LEDs.

### IR Repeating
Automatic repeat functionality for volume and channel commands with customizable repeat rates.

## API Usage

### Send IR Command

```http
POST /api/devices/{deviceId}/commands
Content-Type: application/json

{
  "command": "power",
  "repeat": 1
}
```

### Learn IR Code

```http
POST /api/devices/{deviceId}/learn
Content-Type: application/json

{
  "commandName": "custom1",
  "timeout": 10000
}
```

### Test IR Output

```http
POST /api/hardware/ir/test
Content-Type: application/json

{
  "protocol": "NEC",
  "code": "0xE0E040BF",
  "repeat": 3
}
```