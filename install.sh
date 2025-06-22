#!/bin/bash
set -e

# Zapper Installation Script
# One-line install: curl -sSL https://raw.githubusercontent.com/yourusername/zapper-next-gen/main/install.sh | bash

VERSION="${VERSION:-latest}"
REPO="yourusername/zapper-next-gen"
INSTALL_DIR="/opt/zapper"

echo "🔌 Installing Zapper Universal Remote Control..."
echo ""

# Check if running on Raspberry Pi
if [[ -f /proc/device-tree/model ]] && grep -q "Raspberry Pi" /proc/device-tree/model; then
    echo "✅ Raspberry Pi detected"
    IS_RASPBERRY_PI=true
else
    echo "ℹ️  Not running on Raspberry Pi - some features may be limited"
    IS_RASPBERRY_PI=false
fi

# Detect architecture
ARCH=$(uname -m)
case $ARCH in
    x86_64) 
        RUNTIME="linux-x64"
        echo "✅ Architecture: x64"
        ;;
    aarch64) 
        RUNTIME="linux-arm64"
        echo "✅ Architecture: ARM64"
        ;;
    *) 
        echo "❌ Unsupported architecture: $ARCH"
        exit 1
        ;;
esac

# Check for required dependencies
echo ""
echo "📦 Checking dependencies..."

# Check for .NET runtime
if ! command -v dotnet &> /dev/null; then
    echo "⚠️  .NET runtime not found. Installing..."
    
    # Install .NET 9.0
    curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 9.0 --runtime aspnetcore
    export DOTNET_ROOT=$HOME/.dotnet
    export PATH=$PATH:$HOME/.dotnet
    
    # Add to bashrc for permanent installation
    echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.bashrc
    echo 'export PATH=$PATH:$HOME/.dotnet' >> ~/.bashrc
else
    echo "✅ .NET runtime found"
fi

# Get latest release version if not specified
if [ "$VERSION" = "latest" ]; then
    echo ""
    echo "🔍 Finding latest release..."
    VERSION=$(curl -s "https://api.github.com/repos/$REPO/releases/latest" | grep '"tag_name":' | sed -E 's/.*"([^"]+)".*/\1/')
    
    if [ -z "$VERSION" ]; then
        echo "❌ Could not determine latest version"
        exit 1
    fi
fi

echo "📥 Downloading Zapper $VERSION for $RUNTIME..."

# Create temporary directory
TEMP_DIR=$(mktemp -d)
cd "$TEMP_DIR"

# Download release
DOWNLOAD_URL="https://github.com/$REPO/releases/download/$VERSION/zapper-$VERSION-$RUNTIME.tar.gz"

if ! curl -sL "$DOWNLOAD_URL" | tar -xz; then
    echo "❌ Failed to download release"
    echo "   URL: $DOWNLOAD_URL"
    exit 1
fi

# Install application
echo ""
echo "📂 Installing to $INSTALL_DIR..."

sudo mkdir -p "$INSTALL_DIR"
sudo cp -r * "$INSTALL_DIR/"
sudo chmod +x "$INSTALL_DIR/Zapper"

# Set up permissions for Raspberry Pi
if [ "$IS_RASPBERRY_PI" = true ]; then
    # Add pi user to necessary groups
    sudo usermod -a -G gpio,input,bluetooth pi 2>/dev/null || true
    
    # Create data directory with proper permissions
    sudo mkdir -p "$INSTALL_DIR/data"
    sudo chown -R pi:pi "$INSTALL_DIR/data"
fi

# Create systemd service
echo ""
echo "🔧 Creating systemd service..."

sudo tee /etc/systemd/system/zapper.service > /dev/null << EOF
[Unit]
Description=Zapper Universal Remote Control
After=network.target

[Service]
Type=notify
User=pi
Group=pi
WorkingDirectory=$INSTALL_DIR
ExecStart=$INSTALL_DIR/Zapper
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=zapper
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://0.0.0.0:5000
Environment=ConnectionStrings__DefaultConnection=Data Source=$INSTALL_DIR/data/zapper.db

[Install]
WantedBy=multi-user.target
EOF

# Reload systemd and enable service
sudo systemctl daemon-reload
sudo systemctl enable zapper

# Clean up
cd /
rm -rf "$TEMP_DIR"

echo ""
echo "✅ Zapper installed successfully!"
echo ""
echo "📋 Next steps:"
echo "   1. Start the service: sudo systemctl start zapper"
echo "   2. Check status: sudo systemctl status zapper"
echo "   3. View logs: sudo journalctl -u zapper -f"
echo "   4. Access web interface: http://localhost:5000"
echo ""

if [ "$IS_RASPBERRY_PI" = true ]; then
    echo "🔌 Hardware setup:"
    echo "   - Connect IR LED to GPIO 18 (see docs/HARDWARE.md)"
    echo "   - Ensure proper permissions (may need to reboot)"
    echo ""
fi

echo "📚 For documentation, visit: https://github.com/$REPO/tree/main/docs"
echo ""
echo "🎉 Happy controlling!"