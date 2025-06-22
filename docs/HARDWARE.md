# Hardware Setup Guide

This guide covers the hardware setup required for ZapperHub, including IR LED circuits, GPIO configuration, and optional components.

## Overview

ZapperHub requires specific hardware components to control devices via infrared signals. This guide will help you build the necessary circuits and configure your Raspberry Pi.

## Required Components

### Basic IR Setup
- **Raspberry Pi 4B** (2GB+ RAM)
- **IR LED** (940nm wavelength recommended)
- **NPN Transistor** (2N2222 or BC547)
- **Resistors** (330Ω for LED, 10kΩ for base)
- **Breadboard or PCB**
- **Jumper wires**

### Optional Components
- **IR Receiver** (for learning codes)
- **USB Remote Control** (for input)
- **External IR Blaster** (for increased range)
- **Power Amplifier** (for multiple IR LEDs)

## IR Transmitter Circuit

### Basic Single LED Circuit

```
                    +3.3V
                      │
                      ├─── 330Ω ─── IR LED Anode
                      │                │
GPIO 18 ─── 10kΩ ─── Base             │
                   Collector ─────────┘
                   Emitter ─── Ground
                  (2N2222 NPN)
```

### Component Details

| Component | Value | Purpose |
|-----------|-------|---------|
| IR LED | 940nm, 1.5V forward voltage | Infrared transmission |
| Transistor | 2N2222 NPN | Current amplification |
| Base Resistor | 10kΩ | Limit base current |
| LED Resistor | 330Ω | Limit LED current |

### Breadboard Layout

```
Raspberry Pi          Breadboard
┌─────────────┐      ┌─────────────────┐
│             │      │  +  IR LED  -   │
│         3.3V├──────┤  │    │     │   │
│      GPIO 18├──────┤  │ 330Ω    │   │
│         GND ├──────┤  │    │     │   │
│             │      │  │    ├─────┘   │
└─────────────┘      │  │ 10kΩ         │
                     │  │    │         │
                     │  │  Base        │
                     │  │Collector     │
                     │  │ Emitter──────┤
                     └─────────────────┘
```

## Advanced IR Circuit

### High-Power Multi-LED Array

For better range and coverage, use multiple IR LEDs:

```
                    +5V (External)
                      │
                   ┌──┴──┐
                   │ TIP │ ← TIP31 Power Transistor
                   │ 31  │
                   └──┬──┘
                      │
         ┌─ 330Ω ─ LED1 ─┐
         ├─ 330Ω ─ LED2 ─┤
         ├─ 330Ω ─ LED3 ─┼─── Ground
         └─ 330Ω ─ LED4 ─┘
                      ▲
GPIO 18 ─── 1kΩ ──────┘
```

### Components for High-Power Setup

| Component | Specification | Quantity |
|-----------|---------------|----------|
| IR LEDs | 940nm, 100mA max | 4-8 |
| Power Transistor | TIP31 NPN | 1 |
| Base Resistor | 1kΩ | 1 |
| LED Resistors | 330Ω | 4-8 |
| External PSU | 5V, 1A | 1 |

## GPIO Configuration

### Pin Mapping

| GPIO Pin | Physical Pin | Function | Default Use |
|----------|--------------|----------|-------------|
| GPIO 18 | Pin 12 | PWM0 | IR Transmitter |
| GPIO 19 | Pin 35 | PWM1 | IR Receiver (optional) |
| GPIO 2 | Pin 3 | I2C SDA | Future expansion |
| GPIO 3 | Pin 5 | I2C SCL | Future expansion |

### Enable GPIO Access

```bash
# Add user to gpio group
sudo usermod -a -G gpio $USER

# Enable SPI and I2C (optional)
sudo raspi-config nonint do_spi 0
sudo raspi-config nonint do_i2c 0

# Reboot to apply changes
sudo reboot
```

### Verify GPIO Setup

```bash
# Check GPIO group membership
groups $USER

# Test GPIO access
ls -la /dev/gpiomem

# Check pinout
pinout
```

## IR LED Testing

### Manual Testing

```bash
# Install wiringpi for testing
sudo apt install wiringpi

# Test IR LED (GPIO 18 = wiringPi pin 1)
gpio mode 1 out
gpio write 1 1  # LED on
gpio write 1 0  # LED off
```

### Multimeter Testing

1. **Voltage Test**: Measure 3.3V between GPIO 18 and ground
2. **Current Test**: Should see ~20-50mA through LED when active
3. **LED Test**: IR LED should glow dimly (visible with phone camera)

### Phone Camera Test

IR LEDs emit light visible to phone cameras:
1. Point phone camera at IR LED
2. Activate GPIO pin
3. LED should appear bright white/purple on camera

## USB Remote Configuration

### Supported USB Remotes

ZapperHub works with standard USB HID remotes:
- **Generic USB IR remotes** from Amazon/eBay
- **Windows Media Center remotes**
- **Logitech Harmony USB receivers**
- **Custom Arduino-based remotes**

### Setup Process

```bash
# Check connected USB devices
lsusb

# Check HID devices
ls /dev/input/

# Test USB remote input
sudo cat /dev/input/event0  # Replace with correct event device
```

### HID Device Permissions

```bash
# Add user to input group
sudo usermod -a -G input $USER

# Create udev rule for automatic permissions
sudo tee /etc/udev/rules.d/99-zapperhub-hid.rules > /dev/null <<'EOF'
KERNEL=="event*", SUBSYSTEM=="input", GROUP="input", MODE="0664"
SUBSYSTEM=="usb", ATTRS{idVendor}=="ffff", ATTRS{idProduct}=="ffff", GROUP="input", MODE="0664"
EOF

# Reload udev rules
sudo udevadm control --reload-rules
sudo udevadm trigger
```

## Power Considerations

### Power Requirements

| Component | Current Draw | Notes |
|-----------|--------------|-------|
| Raspberry Pi 4B | 500-800mA | Base consumption |
| Single IR LED | 20-50mA | During transmission |
| USB Remote | 10-20mA | When active |
| **Total** | **600-900mA** | Typical operation |

### Power Supply Recommendations

- **Minimum**: Official Raspberry Pi 4 PSU (5V, 3A)
- **Recommended**: Quality USB-C PSU with surge protection
- **UPS Option**: Add a UPS for continuous operation

### Power-Saving Features

```bash
# Disable unnecessary services
sudo systemctl disable bluetooth  # If not using Bluetooth devices
sudo systemctl disable wifi       # If using Ethernet

# Reduce GPU memory split
echo "gpu_mem=16" | sudo tee -a /boot/config.txt

# Disable HDMI output (headless operation)
echo "hdmi_blanking=1" | sudo tee -a /boot/config.txt
```

## Enclosure and Mounting

### Recommended Enclosures

1. **Official Raspberry Pi Case** - Basic protection
2. **Flirc Case** - Passive cooling, sleek design
3. **Argon ONE** - Active cooling, easy access
4. **Custom 3D Printed** - Perfect fit for IR circuit

### IR LED Placement

- **Direct Line of Sight**: Best performance to devices
- **Central Location**: Coverage of entire room
- **Multiple Emitters**: Use IR splitters for multiple rooms
- **Avoid Obstacles**: Glass, mirrors can interfere

### Mounting Options

```bash
# Wall mount with GPIO access
# Desk mount for easy access
# Media center integration
# Ceiling mount for wide coverage
```

## Troubleshooting Hardware Issues

### IR LED Not Working

```bash
# Check voltage at GPIO pin
gpio read 1

# Verify circuit continuity
# Check transistor orientation
# Confirm LED polarity (long leg = anode)
```

### GPIO Permission Errors

```bash
# Check group membership
id $USER

# Fix permissions
sudo chown root:gpio /dev/gpiomem
sudo chmod g+rw /dev/gpiomem
```

### USB Remote Not Detected

```bash
# Check USB enumeration
dmesg | grep -i usb

# List input devices
cat /proc/bus/input/devices

# Test raw input
sudo evtest
```

### Power Issues

```bash
# Check power supply voltage
vcgencmd measure_volts

# Check for undervoltage warnings
vcgencmd get_throttled

# Monitor power consumption
# Use multimeter on power supply line
```

## Safety Considerations

### Electrical Safety

- **Never exceed GPIO voltage ratings** (3.3V max)
- **Use appropriate current limiting resistors**
- **Double-check polarity** before connecting power
- **Use ESD protection** when handling components

### IR Safety

- **IR LEDs are generally safe** but avoid direct eye exposure
- **High-power arrays** should have current limiting
- **External power supplies** should be regulated and filtered

## Performance Optimization

### IR Range Enhancement

```bash
# Increase transmission power (within limits)
# Use focusing lens or reflector
# Multiple LEDs in parallel
# External IR blaster with amplifier
```

### GPIO Performance

```bash
# Use hardware PWM for better timing
# Minimize system load during transmission
# Real-time kernel for precise timing
# Dedicated IR transmission core
```

## Future Expansion

### Planned Features

- **IR Learning**: Capture codes from existing remotes
- **RF Transmission**: 433MHz module support
- **Multiple Zones**: Room-by-room control
- **Sensor Integration**: Motion, light, temperature

### Compatible Hardware

- **433MHz Modules**: CC1101, RFM69
- **IR Receivers**: TSOP4838, VS1838B
- **Sensors**: PIR, DHT22, LDR
- **Displays**: OLED, LCD for status

---

**Hardware setup complete! Your ZapperHub is ready for device control.** ⚡