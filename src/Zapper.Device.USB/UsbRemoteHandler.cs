using HidSharp;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Zapper.Device.USB;

public class UsbRemoteHandler : IUsbRemoteHandler, IDisposable
{
    private readonly ILogger<UsbRemoteHandler> _logger;
    private readonly ConcurrentDictionary<string, HidDevice> _connectedDevices = new();
    private readonly ConcurrentDictionary<string, HidStream> _activeStreams = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private bool _isListening;
    private bool _disposed;

    public event EventHandler<RemoteButtonEventArgs>? ButtonPressed;

    public UsbRemoteHandler(ILogger<UsbRemoteHandler> logger)
    {
        _logger = logger;
    }

    public bool IsListening => _isListening;

    public async Task StartListeningAsync(CancellationToken cancellationToken = default)
    {
        if (_isListening || _disposed)
            return;

        _logger.LogInformation("Starting USB remote listener");

        try
        {
            // Discover HID devices that look like remotes
            DiscoverRemoteDevices();

            _isListening = true;

            // Start background task to monitor for new devices
            _ = Task.Run(() => MonitorDevicesAsync(_cancellationTokenSource.Token), cancellationToken);

            _logger.LogInformation("USB remote listener started. Monitoring {DeviceCount} devices", _connectedDevices.Count);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start USB remote listener");
            throw;
        }
    }

    public Task StopListeningAsync()
    {
        if (!_isListening)
            return Task.CompletedTask;

        _logger.LogInformation("Stopping USB remote listener");

        _isListening = false;
        _cancellationTokenSource.Cancel();

        // Close all active streams
        foreach (var stream in _activeStreams.Values)
        {
            try
            {
                stream?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error closing HID stream");
            }
        }

        _activeStreams.Clear();
        _connectedDevices.Clear();

        _logger.LogInformation("USB remote listener stopped");
        return Task.CompletedTask;
    }

    public IEnumerable<string> GetConnectedRemotes()
    {
        return _connectedDevices.Keys.ToList();
    }

    private void DiscoverRemoteDevices()
    {
        var deviceList = DeviceList.Local;
        var hidDevices = deviceList.GetHidDevices();

        foreach (var device in hidDevices)
        {
            if (!IsRemoteDevice(device))
                continue;

            var deviceId = GetDeviceId(device);
            _connectedDevices.TryAdd(deviceId, device);

            try
            {
                var stream = device.Open();

                if (stream == null)
                    continue;

                _activeStreams.TryAdd(deviceId, stream);
                _ = Task.Run(() => ListenToDeviceAsync(deviceId, stream, _cancellationTokenSource.Token));

                _logger.LogInformation("Connected to USB remote: {DeviceId} ({ProductName})",
                    deviceId, device.GetProductName() ?? "Unknown");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to open device {DeviceId}", deviceId);
            }
        }
    }

    private async Task MonitorDevicesAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && _isListening)
        {
            try
            {
                await Task.Delay(5000, cancellationToken); // Check every 5 seconds
                DiscoverRemoteDevices();
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error during device monitoring");
            }
        }
    }

    private async Task ListenToDeviceAsync(string deviceId, HidStream stream, CancellationToken cancellationToken)
    {
        var buffer = new byte[stream.Device.GetMaxInputReportLength()];

        try
        {
            while (!cancellationToken.IsCancellationRequested && _isListening)
            {
                var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);

                if (bytesRead > 0)
                {
                    ProcessInputReport(deviceId, buffer, bytesRead);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when stopping
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error reading from device {DeviceId}", deviceId);

            // Remove device from active list
            _activeStreams.TryRemove(deviceId, out _);
            _connectedDevices.TryRemove(deviceId, out _);
        }
    }

    private void ProcessInputReport(string deviceId, byte[] buffer, int length)
    {
        try
        {
            // Simple key mapping - in reality you'd have device-specific mappings
            var keyCode = buffer[1]; // Assuming second byte contains key code

            if (keyCode == 0)
                return;

            var buttonName = MapKeyCodeToButton(keyCode);
            var eventArgs = new RemoteButtonEventArgs(deviceId, buttonName, keyCode);

            _logger.LogDebug("Button pressed on {DeviceId}: {ButtonName} (0x{KeyCode:X2})",
                deviceId, buttonName, keyCode);

            ButtonPressed?.Invoke(this, eventArgs);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error processing input from device {DeviceId}", deviceId);
        }
    }

    private bool IsRemoteDevice(HidDevice device)
    {
        try
        {
            // Check if device looks like a remote control based on product name and vendor
            var productName = device.GetProductName()?.ToLowerInvariant() ?? "";
            var vendorId = device.VendorID;
            var productId = device.ProductID;

            // Check product name for remote-like keywords
            if (productName.Contains("remote") ||
                productName.Contains("media") ||
                productName.Contains("control") ||
                productName.Contains("receiver"))
            {
                return true;
            }

            // Known remote control vendor/product IDs (examples)
            // This would be expanded with real device IDs
            var knownRemoteVendors = new[] { 0x046D, 0x054C, 0x05AC }; // Logitech, Sony, Apple examples

            return knownRemoteVendors.Contains(vendorId);
        }
        catch
        {
            return false;
        }
    }

    private string GetDeviceId(HidDevice device)
    {
        try
        {
            return $"{device.VendorID:X4}:{device.ProductID:X4}:{device.GetSerialNumber() ?? "unknown"}";
        }
        catch
        {
            return $"{device.VendorID:X4}:{device.ProductID:X4}:unknown";
        }
    }

    private string MapKeyCodeToButton(byte keyCode)
    {
        // Basic key mapping - extend this based on actual remote types
        return keyCode switch
        {
            0x01 => "Power",
            0x02 => "VolumeUp",
            0x03 => "VolumeDown",
            0x04 => "Mute",
            0x05 => "ChannelUp",
            0x06 => "ChannelDown",
            0x07 => "Up",
            0x08 => "Down",
            0x09 => "Left",
            0x0A => "Right",
            0x0B => "OK",
            0x0C => "Menu",
            0x0D => "Back",
            0x0E => "Home",
            0x10 => "Play",
            0x11 => "Pause",
            0x12 => "Stop",
            0x13 => "FastForward",
            0x14 => "Rewind",
            0x15 => "Record",
            0x20 => "Number0",
            0x21 => "Number1",
            0x22 => "Number2",
            0x23 => "Number3",
            0x24 => "Number4",
            0x25 => "Number5",
            0x26 => "Number6",
            0x27 => "Number7",
            0x28 => "Number8",
            0x29 => "Number9",
            _ => $"Unknown_0x{keyCode:X2}"
        };
    }

    public void Dispose()
    {
        if (_disposed) return;

        _cancellationTokenSource.Cancel();

        foreach (var stream in _activeStreams.Values)
        {
            stream?.Dispose();
        }

        _cancellationTokenSource.Dispose();
        _disposed = true;

        _logger.LogInformation("USB remote handler disposed");
    }
}