# Installation Guide

This guide provides detailed instructions for installing ZapperHub on your Raspberry Pi.

## Prerequisites

### Hardware Requirements
- Raspberry Pi 4B (2GB RAM minimum, 4GB recommended)
- MicroSD card (16GB minimum, Class 10)
- IR LED circuit for infrared transmission
- Network connection (Ethernet preferred)
- Optional: USB remote control

### Software Requirements
- Raspberry Pi OS (Bullseye 64-bit or newer)
- .NET 9.0 Runtime
- Git

## Step 1: Prepare Raspberry Pi OS

### 1.1 Flash Raspberry Pi OS

1. Download [Raspberry Pi Imager](https://www.raspberrypi.com/software/)
2. Flash **Raspberry Pi OS (64-bit)** to your SD card
3. Enable SSH in the imager settings (optional but recommended)
4. Insert SD card and boot your Raspberry Pi

### 1.2 Initial Setup

```bash
# Update system packages
sudo apt update && sudo apt upgrade -y

# Install required system packages
sudo apt install -y curl wget git build-essential

# Enable GPIO and SPI (for hardware control)
sudo raspi-config nonint do_spi 0
sudo raspi-config nonint do_ssh 0

# Reboot to apply changes
sudo reboot
```

## Step 2: Install .NET 9.0

### 2.1 Download and Install .NET

```bash
# Download the .NET install script
curl -sSL https://dot.net/v1/dotnet-install.sh -o dotnet-install.sh
chmod +x dotnet-install.sh

# Install .NET 9.0
./dotnet-install.sh --channel 9.0

# Add .NET to PATH
echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.bashrc
echo 'export PATH=$PATH:$HOME/.dotnet' >> ~/.bashrc
source ~/.bashrc

# Verify installation
dotnet --version
```

### 2.2 Alternative: Package Manager Installation

```bash
# Add Microsoft package repository
wget https://packages.microsoft.com/config/debian/11/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# Install .NET runtime
sudo apt update
sudo apt install -y dotnet-runtime-9.0
```

## Step 3: Install ZapperHub

### 3.1 Clone Repository

```bash
# Create application directory
sudo mkdir -p /opt/zapperhub
sudo chown $USER:$USER /opt/zapperhub
cd /opt/zapperhub

# Clone the repository
git clone https://github.com/yourusername/zapper-next-gen.git .
```

### 3.2 Build Application

```bash
# Navigate to source directory
cd src

# Restore NuGet packages
dotnet restore

# Build in release mode
dotnet build --configuration Release

# Publish self-contained for Raspberry Pi
dotnet publish ZapperHub/ZapperHub.csproj \
  --configuration Release \
  --runtime linux-arm64 \
  --self-contained true \
  --output /opt/zapperhub/publish
```

## Step 4: Configure System Service

### 4.1 Create Service File

```bash
# Create systemd service file
sudo tee /etc/systemd/system/zapperhub.service > /dev/null <<'EOF'
[Unit]
Description=ZapperHub Universal Remote Control
After=network.target

[Service]
Type=notify
User=pi
Group=pi
WorkingDirectory=/opt/zapperhub/publish
ExecStart=/opt/zapperhub/publish/ZapperHub
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=zapperhub
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://0.0.0.0:5000

[Install]
WantedBy=multi-user.target
EOF
```

### 4.2 Enable and Start Service

```bash
# Reload systemd daemon
sudo systemctl daemon-reload

# Enable service to start on boot
sudo systemctl enable zapperhub

# Start the service
sudo systemctl start zapperhub

# Check service status
sudo systemctl status zapperhub
```

## Step 5: Configure Firewall (Optional)

```bash
# Install UFW (if not already installed)
sudo apt install -y ufw

# Allow ZapperHub port
sudo ufw allow 5000/tcp

# Allow SSH (if using)
sudo ufw allow ssh

# Enable firewall
sudo ufw --force enable
```

## Step 6: Hardware Setup

### 6.1 IR LED Circuit

Connect an IR LED to GPIO pin 18 with a current-limiting resistor:

```
Raspberry Pi GPIO 18 ─── 330Ω Resistor ─── IR LED Anode
                                           IR LED Cathode ─── Ground (Pin 6)
```

### 6.2 Test IR Transmission

```bash
# Check GPIO access
ls -la /dev/gpiomem

# If permission denied, add user to gpio group
sudo usermod -a -G gpio $USER

# Reboot to apply group changes
sudo reboot
```

## Step 7: Access Web Interface

### 7.1 Local Access

Open a web browser and navigate to:
- `http://localhost:5000` (from Raspberry Pi)
- `http://[pi-ip-address]:5000` (from other devices)

### 7.2 Find Raspberry Pi IP Address

```bash
# Show IP address
hostname -I

# Or use ip command
ip addr show wlan0 | grep 'inet '
```

## Step 8: Initial Configuration

### 8.1 Add Your First Device

1. Open the web interface
2. Navigate to **Devices** tab
3. Click **"Add Device"**
4. Fill in device information:
   - Name: "Living Room TV"
   - Brand: "Samsung"
   - Model: "Generic TV"
   - Type: "Television"
   - Connection: "Infrared (IR)"
5. Click **"Add Device"**

### 8.2 Test Device Control

1. Go to **Remote** tab
2. Select your device from dropdown
3. Try the power button
4. Verify device responds

## Troubleshooting

### Service Won't Start

```bash
# Check service logs
sudo journalctl -u zapperhub -f

# Check application logs
sudo journalctl -u zapperhub --since "1 hour ago"
```

### Permission Issues

```bash
# Fix file permissions
sudo chown -R pi:pi /opt/zapperhub
sudo chmod +x /opt/zapperhub/publish/ZapperHub

# Fix GPIO permissions
sudo usermod -a -G gpio pi
```

### Network Issues

```bash
# Check if port is listening
sudo netstat -tlnp | grep :5000

# Test local connection
curl http://localhost:5000/api/system/status
```

### IR Not Working

```bash
# Check GPIO is available
cat /proc/device-tree/soc/gpio*/status

# Verify IR LED circuit
# LED should be on GPIO 18 (physical pin 12)
```

## Advanced Configuration

### Environment Variables

Create `/opt/zapperhub/publish/appsettings.Production.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=/opt/zapperhub/data/zapper.db"
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
      "Microsoft.AspNetCore": "Warning",
      "ZapperHub": "Debug"
    }
  },
  "AllowedHosts": "*"
}
```

### Auto-Updates

Create update script `/opt/zapperhub/update.sh`:

```bash
#!/bin/bash
cd /opt/zapperhub
git pull origin main
cd src
dotnet publish ZapperHub/ZapperHub.csproj \
  --configuration Release \
  --runtime linux-arm64 \
  --self-contained true \
  --output /opt/zapperhub/publish
sudo systemctl restart zapperhub
```

### Backup Configuration

```bash
# Backup database and configuration
sudo cp /opt/zapperhub/publish/zapper.db /opt/zapperhub/backup/
sudo cp /opt/zapperhub/publish/appsettings.Production.json /opt/zapperhub/backup/
```

## Next Steps

- [Hardware Setup Guide](HARDWARE.md) - Detailed IR circuit instructions
- [Configuration Guide](CONFIGURATION.md) - Device and activity setup
- [API Documentation](API.md) - Integration options
- [Troubleshooting Guide](TROUBLESHOOTING.md) - Common issues

## Support

If you encounter issues during installation:

1. Check the [Troubleshooting Guide](TROUBLESHOOTING.md)
2. Review service logs: `sudo journalctl -u zapperhub -f`
3. Open an issue on [GitHub](https://github.com/yourusername/zapper-next-gen/issues)

---

**Installation complete! Your ZapperHub is ready to use.** 🎉