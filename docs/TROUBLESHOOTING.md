# Troubleshooting Guide

This guide helps you diagnose and resolve common issues with ZapperHub.

## Common Issues

### 1. Service Won't Start

#### Symptoms
- ZapperHub service fails to start
- Web interface not accessible
- Service shows "failed" status

#### Diagnosis

```bash
# Check service status
sudo systemctl status zapperhub

# View service logs
sudo journalctl -u zapperhub -f

# Check application logs
sudo journalctl -u zapperhub --since "1 hour ago"
```

#### Solutions

**Permission Issues:**
```bash
# Fix file ownership
sudo chown -R pi:pi /opt/zapperhub
sudo chmod +x /opt/zapperhub/publish/ZapperHub

# Fix GPIO permissions
sudo usermod -a -G gpio pi
sudo reboot
```

**Missing Dependencies:**
```bash
# Install .NET runtime
sudo apt install -y dotnet-runtime-9.0

# Verify .NET installation
dotnet --version
```

**Port Already in Use:**
```bash
# Check what's using port 5000
sudo netstat -tlnp | grep :5000

# Kill conflicting process
sudo kill $(sudo lsof -ti:5000)
```

### 2. IR Commands Not Working

#### Symptoms
- Devices don't respond to IR commands
- No visible IR LED activity
- Commands sent but no effect

#### Diagnosis

```bash
# Check GPIO permissions
ls -la /dev/gpiomem

# Test GPIO manually
gpio mode 1 out
gpio write 1 1
gpio write 1 0

# Check service logs for IR errors
sudo journalctl -u zapperhub | grep -i "infrared\|IR"
```

#### Solutions

**Circuit Issues:**
- Verify IR LED polarity (long leg = anode)
- Check resistor values (330Ω for LED, 10kΩ for base)
- Ensure transistor is connected correctly
- Test with multimeter (should see voltage changes)

**GPIO Issues:**
```bash
# Add user to gpio group
sudo usermod -a -G gpio pi

# Enable GPIO access
echo 'dtparam=spi=on' | sudo tee -a /boot/config.txt
echo 'dtparam=i2c=on' | sudo tee -a /boot/config.txt

# Reboot after changes
sudo reboot
```

**Configuration Issues:**
```bash
# Check appsettings.json
cat /opt/zapperhub/publish/appsettings.json

# Verify GPIO pin configuration
# Default should be pin 18
```

### 3. WebOS TV Discovery Fails

#### Symptoms
- LG TVs not discovered automatically
- Discovery returns empty results
- Network connection issues

#### Diagnosis

```bash
# Check network connectivity
ping [tv-ip-address]

# Test WebOS port
telnet [tv-ip-address] 3000

# Check firewall settings
sudo ufw status
```

#### Solutions

**Network Configuration:**
```bash
# Ensure TV and Pi are on same subnet
ip route show

# Check for network isolation (guest networks)
# Ensure multicast/broadcast is allowed
```

**TV Settings:**
1. Enable "LG Connect Apps" on TV
2. Allow "Mobile TV On" setting
3. Check TV network settings
4. Restart TV networking

**Manual Connection:**
```bash
# Add device manually with IP address
curl -X POST http://localhost:5000/api/devices \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Living Room TV",
    "brand": "LG",
    "connectionType": 6,
    "ipAddress": "192.168.1.100"
  }'
```

### 4. Web Interface Not Loading

#### Symptoms
- Browser shows "connection refused"
- Page loads but shows errors
- Slow or unresponsive interface

#### Diagnosis

```bash
# Check if service is running
sudo systemctl is-active zapperhub

# Test local connection
curl http://localhost:5000/api/system/status

# Check network access
curl http://[pi-ip]:5000/api/system/status
```

#### Solutions

**Service Issues:**
```bash
# Restart service
sudo systemctl restart zapperhub

# Check service configuration
sudo systemctl cat zapperhub
```

**Network Issues:**
```bash
# Check firewall
sudo ufw status
sudo ufw allow 5000/tcp

# Check listening ports
sudo netstat -tlnp | grep :5000
```

**Browser Issues:**
- Clear browser cache and cookies
- Try different browser or incognito mode
- Check browser console for JavaScript errors
- Verify browser supports WebSockets (for SignalR)

### 5. Bluetooth Devices Not Working

#### Symptoms
- Bluetooth devices not discovered
- Pairing fails
- Commands don't reach device

#### Diagnosis

```bash
# Check Bluetooth service
sudo systemctl status bluetooth

# Scan for devices
bluetoothctl scan on

# Check Bluetooth adapter
hciconfig
```

#### Solutions

**Enable Bluetooth:**
```bash
# Start Bluetooth service
sudo systemctl enable bluetooth
sudo systemctl start bluetooth

# Reset Bluetooth adapter
sudo hciconfig hci0 down
sudo hciconfig hci0 up
```

**Pairing Issues:**
```bash
# Put device in pairing mode
# Use bluetoothctl for manual pairing
bluetoothctl
power on
agent on
scan on
pair [device-mac]
connect [device-mac]
```

### 6. Database Issues

#### Symptoms
- Device configurations lost
- Activities not saving
- Database corruption errors

#### Diagnosis

```bash
# Check database file
ls -la /opt/zapperhub/publish/zapper.db

# Test database connectivity
sqlite3 /opt/zapperhub/publish/zapper.db ".tables"

# Check database logs
sudo journalctl -u zapperhub | grep -i "database\|sqlite"
```

#### Solutions

**Permissions:**
```bash
# Fix database permissions
sudo chown pi:pi /opt/zapperhub/publish/zapper.db
sudo chmod 664 /opt/zapperhub/publish/zapper.db
```

**Corruption Recovery:**
```bash
# Backup existing database
cp /opt/zapperhub/publish/zapper.db /opt/zapperhub/zapper.db.backup

# Try to repair
sqlite3 /opt/zapperhub/publish/zapper.db ".recover" > recovered.sql
sqlite3 /opt/zapperhub/publish/zapper_new.db < recovered.sql

# Restore from backup if needed
# (Create new database if all else fails)
```

### 7. Performance Issues

#### Symptoms
- Slow response times
- High CPU/memory usage
- Web interface lag

#### Diagnosis

```bash
# Check system resources
htop
free -h
df -h

# Check service resource usage
sudo systemctl status zapperhub

# Monitor network traffic
sudo netstat -i
```

#### Solutions

**Resource Optimization:**
```bash
# Reduce logging level
# Edit appsettings.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  }
}

# Disable unnecessary services
sudo systemctl disable bluetooth  # if not needed
sudo systemctl disable wifi       # if using ethernet
```

**Hardware Optimization:**
```bash
# Increase GPU memory split for better performance
echo "gpu_mem=64" | sudo tee -a /boot/config.txt

# Enable hardware acceleration
echo "dtoverlay=vc4-kms-v3d" | sudo tee -a /boot/config.txt

# Reboot after changes
sudo reboot
```

## Diagnostic Commands

### System Information

```bash
# System overview
uname -a
cat /etc/os-release
vcgencmd measure_temp
vcgencmd measure_volts
vcgencmd get_throttled

# Memory and storage
free -h
df -h
lsblk

# Network configuration
ip addr show
ip route show
cat /etc/resolv.conf
```

### Service Diagnostics

```bash
# Service status and logs
sudo systemctl status zapperhub --no-pager -l
sudo journalctl -u zapperhub --since "1 hour ago" --no-pager

# Process information
ps aux | grep ZapperHub
lsof -p $(pgrep ZapperHub)

# Network connections
sudo netstat -tlnp | grep ZapperHub
sudo ss -tlnp | grep :5000
```

### Hardware Diagnostics

```bash
# GPIO status
pinout
gpio readall

# USB devices
lsusb -v
lsusb -t

# Bluetooth
hciconfig -a
bluetoothctl show

# I2C and SPI
ls /dev/i2c*
ls /dev/spi*
```

## Log Analysis

### Important Log Patterns

**Successful Startup:**
```
info: ZapperHub.Program[0] Starting ZapperHub
info: Microsoft.Hosting.Lifetime[14] Now listening on: http://0.0.0.0:5000
info: ZapperHub.Services.IRCodeService[0] Seeded 3 default IR code sets
```

**IR Transmission:**
```
info: ZapperHub.Hardware.MockInfraredTransmitter[0] Mock transmitting IR code: Samsung Generic TV Power - 0xE0E040BF
```

**Device Discovery:**
```
info: ZapperHub.Hardware.WebOSDiscovery[0] Discovered WebOS device: LG TV at 192.168.1.100
```

### Error Patterns

**Permission Errors:**
```
System.UnauthorizedAccessException: Access to the path '/dev/gpiomem' is denied
```
Solution: Add user to gpio group

**Network Errors:**
```
System.Net.NetworkInformation.PingException: An exception occurred during a Ping request
```
Solution: Check network connectivity and firewall

**Database Errors:**
```
Microsoft.Data.Sqlite.SqliteException: SQLite Error 14: 'unable to open database file'
```
Solution: Check database file permissions

## Recovery Procedures

### Complete Reset

```bash
# Stop service
sudo systemctl stop zapperhub

# Backup current configuration
sudo cp -r /opt/zapperhub /opt/zapperhub.backup.$(date +%Y%m%d)

# Reset to defaults
sudo rm /opt/zapperhub/publish/zapper.db
sudo rm /opt/zapperhub/publish/appsettings.Production.json

# Restart service (will recreate database)
sudo systemctl start zapperhub
```

### Configuration Backup/Restore

```bash
# Backup
sudo cp /opt/zapperhub/publish/zapper.db ~/zapperhub-backup-$(date +%Y%m%d).db

# Restore
sudo systemctl stop zapperhub
sudo cp ~/zapperhub-backup-YYYYMMDD.db /opt/zapperhub/publish/zapper.db
sudo chown pi:pi /opt/zapperhub/publish/zapper.db
sudo systemctl start zapperhub
```

## Getting Help

### Before Asking for Help

1. **Check this troubleshooting guide**
2. **Review system logs**: `sudo journalctl -u zapperhub --since "1 hour ago"`
3. **Test basic functionality**: Can you access the web interface?
4. **Document your setup**: Hardware, OS version, network configuration

### Information to Include

When reporting issues, please include:

```bash
# System information
uname -a
cat /etc/os-release
dotnet --version

# Service status
sudo systemctl status zapperhub --no-pager

# Recent logs (last 50 lines)
sudo journalctl -u zapperhub -n 50 --no-pager

# Hardware configuration
pinout
lsusb
```

### Support Channels

- **GitHub Issues**: [Project Issues](https://github.com/yourusername/zapper-next-gen/issues)
- **GitHub Discussions**: [Community Help](https://github.com/yourusername/zapper-next-gen/discussions)
- **Documentation**: [Project Wiki](https://github.com/yourusername/zapper-next-gen/wiki)

### Creating a Minimal Test Case

When reporting bugs:

1. **Isolate the issue**: Does it happen with a single device?
2. **Simplify configuration**: Remove unnecessary devices/activities
3. **Document steps**: Exact sequence to reproduce the problem
4. **Include logs**: Relevant log entries around the time of the issue

---

**Most issues can be resolved by checking logs and verifying configuration. Don't hesitate to ask for help if you're stuck!** 🔧