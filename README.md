# ZapperHub

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com/)
[![Platform](https://img.shields.io/badge/Platform-Raspberry%20Pi-red.svg)](https://www.raspberrypi.org/)

**ZapperHub** is an open-source universal remote control system designed to replace Logitech Harmony hubs. Built with ASP.NET Core and designed to run on Raspberry Pi, ZapperHub provides a modern web interface for controlling your home theater devices through infrared, network, and Bluetooth connections.

## ✨ Features

- **Universal Device Control**: Support for TVs, receivers, cable boxes, streaming devices, and more
- **Multiple Connection Types**: Infrared (IR), Radio Frequency (RF), Network (TCP/WebSocket), Bluetooth, and WebOS
- **Activity-Based Control**: Create one-touch activities like "Watch TV" or "Movie Mode"
- **Real-Time Updates**: Live status updates using SignalR
- **Device Discovery**: Automatic discovery of WebOS TVs and Bluetooth devices
- **IR Code Database**: Comprehensive library of IR codes for popular brands (Samsung, LG, Sony)
- **Modern Web UI**: Responsive interface with device management and virtual remote
- **RESTful API**: Complete API for integration with other systems
- **Raspberry Pi Optimized**: Efficient GPIO control for IR transmission

## 🛠️ Hardware Requirements

### Minimum Requirements
- **Raspberry Pi 4B** (2GB RAM minimum, 4GB recommended)
- **MicroSD Card** (16GB minimum, Class 10)
- **IR LED Circuit** for infrared transmission
- **USB Remote** (optional, for input)

### Recommended Hardware
- **Raspberry Pi 4B** (4GB or 8GB)
- **High-quality MicroSD Card** (32GB, Class 10 or better)
- **IR LED + Transistor Circuit** on GPIO pin 18
- **USB Remote Control** for input
- **Ethernet Connection** (for best performance)

### IR Circuit Diagram
```
GPIO 18 ────┬─── 330Ω Resistor ─── Transistor Base
            │
            └─── IR LED Anode
                 IR LED Cathode ─── Ground
```

## 📋 Software Requirements

- **.NET 9.0 Runtime**
- **SQLite** (included)
- **Raspberry Pi OS** (Bullseye or newer)
- **Web Browser** (Chrome, Firefox, Safari, Edge)

## 🚀 Quick Start

### 1. Install .NET 9.0 on Raspberry Pi

```bash
# Download and install .NET 9.0
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 9.0
echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.bashrc
echo 'export PATH=$PATH:$HOME/.dotnet' >> ~/.bashrc
source ~/.bashrc
```

### 2. Clone and Build ZapperHub

```bash
# Clone the repository
git clone https://github.com/yourusername/zapper-next-gen.git
cd zapper-next-gen

# Build the application
cd src
dotnet build --configuration Release

# Publish for Raspberry Pi
dotnet publish ZapperHub/ZapperHub.csproj -c Release -r linux-arm64 --self-contained
```

### 3. Run ZapperHub

```bash
# Navigate to published output
cd ZapperHub/bin/Release/net9.0/linux-arm64/publish

# Make executable
chmod +x ZapperHub

# Run the application
sudo ./ZapperHub
```

### 4. Access the Web Interface

Open your web browser and navigate to:
- **Local**: http://localhost:5000
- **Network**: http://[raspberry-pi-ip]:5000

## 📖 Documentation

- [Installation Guide](docs/INSTALLATION.md) - Detailed setup instructions
- [Hardware Setup](docs/HARDWARE.md) - IR circuit and GPIO configuration
- [Configuration](docs/CONFIGURATION.md) - Device and activity setup
- [API Reference](docs/API.md) - REST API documentation
- [Troubleshooting](docs/TROUBLESHOOTING.md) - Common issues and solutions
- [Contributing](docs/CONTRIBUTING.md) - Development guidelines

## 🎮 Usage

### Adding Devices

1. **Navigate to Devices** in the web interface
2. **Click "Add Device"** and fill in device details
3. **Select Connection Type**:
   - **Infrared**: For traditional remote-controlled devices
   - **WebOS**: For LG smart TVs (auto-discovery available)
   - **Bluetooth**: For Android TV and Apple TV
   - **Network**: For IP-controlled devices
4. **Test the device** to ensure it responds correctly

### Creating Activities

1. **Go to Activities** section
2. **Click "Add Activity"** and name your activity
3. **Add device commands** in sequence
4. **Set delays** between commands if needed
5. **Test the activity** to verify it works correctly

### Using the Remote

1. **Select a device** from the Remote section
2. **Use the virtual remote** to control your device
3. **Common controls include**:
   - Power, volume, channels
   - Directional pad and OK button
   - Media controls (play, pause, stop)

## 🔧 Configuration

### Environment Variables

```bash
# Database connection
ConnectionStrings__DefaultConnection="Data Source=zapper.db"

# Logging level
Logging__LogLevel__Default="Information"

# GPIO pin for IR transmitter (default: 18)
Hardware__IRTransmitter__GpioPin=18

# Enable hardware features
Hardware__EnableGPIO=true
Hardware__EnableBluetooth=true
```

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=zapper.db"
  },
  "Hardware": {
    "IRTransmitter": {
      "GpioPin": 18,
      "Enabled": true
    },
    "Bluetooth": {
      "Enabled": true
    }
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

## 🔌 Supported Devices

### Television Brands
- **Samsung** - Full IR code support
- **LG** - WebOS smart TVs + IR codes
- **Sony** - Full IR code support
- **Panasonic** - Basic IR support
- **TCL, Hisense, Vizio** - Basic IR support

### Streaming Devices
- **Apple TV** - Bluetooth control
- **Android TV** - Bluetooth control
- **Roku** - Network control
- **Fire TV** - Network control

### Audio Equipment
- **Denon, Yamaha** - Network control
- **Onkyo, Pioneer** - IR control
- **Sonos** - Network control

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guide](docs/CONTRIBUTING.md) for details.

### Development Setup

```bash
# Clone the repository
git clone https://github.com/yourusername/zapper-next-gen.git
cd zapper-next-gen

# Install dependencies
cd src
dotnet restore

# Run in development mode
dotnet run --project ZapperHub
```

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- **Logitech Harmony** - Inspiration for this project
- **LG Electronics** - WebOS protocol documentation
- **.NET Community** - Amazing framework and tools
- **Raspberry Pi Foundation** - Excellent hardware platform

## 📞 Support

- **Issues**: [GitHub Issues](https://github.com/yourusername/zapper-next-gen/issues)
- **Discussions**: [GitHub Discussions](https://github.com/yourusername/zapper-next-gen/discussions)
- **Wiki**: [Project Wiki](https://github.com/yourusername/zapper-next-gen/wiki)

---

**Built with ❤️ for the home automation community**