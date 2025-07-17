using HidSharp;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Zapper.Core.Models;

namespace Zapper.Device.USB;

public class UsbRemoteHandler : IUsbRemoteHandler, IDisposable
{
    private readonly ILogger<UsbRemoteHandler> _logger;
    private readonly ConcurrentDictionary<string, HidDevice> _connectedDevices = new();
    private readonly ConcurrentDictionary<string, HidStream> _activeStreams = new();
    private readonly ConcurrentDictionary<string, ButtonState> _buttonStates = new();
    private readonly ConcurrentDictionary<string, RemoteConfiguration> _remoteConfigurations = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private bool _isListening;
    private bool _disposed;

    public event EventHandler<RemoteButtonEventArgs>? ButtonPressed;
    public event EventHandler<RemoteButtonEventArgs>? ButtonDown;
    public event EventHandler<RemoteButtonEventArgs>? ButtonUp;
    public event EventHandler<RemoteButtonEventArgs>? ButtonLongPress;
    public event EventHandler<string>? RemoteConnected;
    public event EventHandler<string>? RemoteDisconnected;

    public UsbRemoteHandler(ILogger<UsbRemoteHandler> logger)
    {
        _logger = logger;
    }

    public bool IsListening => _isListening;

    public async Task StartListening(CancellationToken cancellationToken = default)
    {
        if (_isListening || _disposed)
            return;

        _logger.LogInformation("Starting USB remote listener");

        try
        {
            // Discover HID devices that look like remotes
            DiscoverRemoteDevices();

            _isListening = true;

            // Start background tasks
            _ = Task.Run(() => MonitorDevicesAsync(_cancellationTokenSource.Token), cancellationToken);
            _ = Task.Run(() => ProcessLongPressAsync(_cancellationTokenSource.Token), cancellationToken);

            _logger.LogInformation("USB remote listener started. Monitoring {DeviceCount} devices", _connectedDevices.Count);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start USB remote listener");
            throw;
        }
    }

    public Task StopListening()
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
        _buttonStates.Clear();
        _remoteConfigurations.Clear();

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

            // Check if device is already connected
            if (_connectedDevices.ContainsKey(deviceId))
                continue;

            if (!_connectedDevices.TryAdd(deviceId, device))
                continue;

            try
            {
                var stream = device.Open();

                if (stream == null)
                {
                    _connectedDevices.TryRemove(deviceId, out _);
                    continue;
                }

                if (!_activeStreams.TryAdd(deviceId, stream))
                {
                    stream.Dispose();
                    _connectedDevices.TryRemove(deviceId, out _);
                    continue;
                }

                _ = Task.Run(() => ListenToDeviceAsync(deviceId, stream, _cancellationTokenSource.Token));

                _logger.LogInformation("Connected to USB remote: {DeviceId} ({ProductName})",
                    deviceId, device.GetProductName() ?? "Unknown");

                // Initialize remote configuration with defaults
                _remoteConfigurations.TryAdd(deviceId, new RemoteConfiguration());

                // Raise connected event
                RemoteConnected?.Invoke(this, deviceId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to open device {DeviceId}", deviceId);
                _connectedDevices.TryRemove(deviceId, out _);
                _activeStreams.TryRemove(deviceId, out var stream);
                stream?.Dispose();
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

                // Check for disconnected devices
                var currentDeviceIds = new HashSet<string>();
                var deviceList = DeviceList.Local;
                var hidDevices = deviceList.GetHidDevices();

                foreach (var device in hidDevices)
                {
                    if (IsRemoteDevice(device))
                    {
                        currentDeviceIds.Add(GetDeviceId(device));
                    }
                }

                // Remove devices that are no longer present
                var disconnectedDevices = _connectedDevices.Keys.Where(id => !currentDeviceIds.Contains(id)).ToList();
                foreach (var deviceId in disconnectedDevices)
                {
                    HandleDeviceDisconnection(deviceId);
                }

                // Discover new devices
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
            HandleDeviceDisconnection(deviceId);
        }
    }

    private void HandleDeviceDisconnection(string deviceId)
    {
        // Remove device from active list
        if (_activeStreams.TryRemove(deviceId, out var stream))
        {
            try
            {
                stream?.Dispose();
            }
            catch { }
        }

        _connectedDevices.TryRemove(deviceId, out _);

        // Remove all button states for this device
        var keysToRemove = _buttonStates.Keys.Where(k => k.StartsWith(deviceId + "_")).ToList();
        foreach (var key in keysToRemove)
        {
            _buttonStates.TryRemove(key, out _);
        }

        _remoteConfigurations.TryRemove(deviceId, out _);

        _logger.LogInformation("USB remote disconnected: {DeviceId}", deviceId);

        // Raise disconnected event
        RemoteDisconnected?.Invoke(this, deviceId);
    }

    private void ProcessInputReport(string deviceId, byte[] buffer, int length)
    {
        try
        {
            // Simple key mapping - in reality you'd have device-specific mappings
            var keyCode = buffer[1]; // Assuming second byte contains key code
            var stateKey = $"{deviceId}_{keyCode}";
            var buttonName = MapKeyCodeToButton(keyCode);
            var config = _remoteConfigurations.GetValueOrDefault(deviceId) ?? new RemoteConfiguration();

            // Handle key state transitions
            var currentState = _buttonStates.GetValueOrDefault(stateKey);
            var isPressed = keyCode != 0;

            if (isPressed && (currentState == null || !currentState.IsPressed))
            {
                // Key down event
                var newState = new ButtonState
                {
                    DeviceId = deviceId,
                    KeyCode = keyCode,
                    ButtonName = buttonName,
                    IsPressed = true,
                    PressStartTime = DateTime.UtcNow,
                    LastRepeatTime = DateTime.UtcNow
                };
                _buttonStates.AddOrUpdate(stateKey, newState, (k, v) => newState);

                var eventArgs = new RemoteButtonEventArgs(
                    deviceId, buttonName, keyCode,
                    ButtonEventType.KeyDown,
                    rawData: buffer.Take(length).ToArray());

                // Check if should intercept
                if (config.EnableInterception && IsSystemButton(keyCode))
                {
                    eventArgs.ShouldIntercept = true;
                }

                _logger.LogDebug("Button down on {DeviceId}: {ButtonName} (0x{KeyCode:X2})",
                    deviceId, buttonName, keyCode);

                ButtonDown?.Invoke(this, eventArgs);
            }
            else if (!isPressed && currentState != null && currentState.IsPressed)
            {
                // Key up event
                var holdDuration = DateTime.UtcNow - currentState.PressStartTime;
                _buttonStates.TryRemove(stateKey, out _);

                var eventArgs = new RemoteButtonEventArgs(
                    deviceId, buttonName, currentState.KeyCode,
                    ButtonEventType.KeyUp,
                    holdDuration: holdDuration,
                    rawData: buffer.Take(length).ToArray());

                _logger.LogDebug("Button up on {DeviceId}: {ButtonName} (0x{KeyCode:X2}) held for {Duration}ms",
                    deviceId, buttonName, keyCode, holdDuration.TotalMilliseconds);

                ButtonUp?.Invoke(this, eventArgs);

                // Also fire a press event if it wasn't a long press
                if (holdDuration.TotalMilliseconds < config.LongPressTimeoutMs)
                {
                    var pressEventArgs = new RemoteButtonEventArgs(
                        deviceId, buttonName, currentState.KeyCode,
                        ButtonEventType.KeyPress,
                        holdDuration: holdDuration,
                        rawData: buffer.Take(length).ToArray());

                    ButtonPressed?.Invoke(this, pressEventArgs);
                }
            }
            else if (isPressed && currentState != null && currentState.IsPressed)
            {
                // Key repeat - update last repeat time
                currentState.LastRepeatTime = DateTime.UtcNow;
            }
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

    public void ConfigureLongPressTimeout(string deviceId, int timeoutMs)
    {
        if (_remoteConfigurations.TryGetValue(deviceId, out var config))
        {
            config.LongPressTimeoutMs = timeoutMs;
            _logger.LogInformation("Set long press timeout for {DeviceId} to {Timeout}ms", deviceId, timeoutMs);
        }
    }

    public void ConfigureButtonInterception(string deviceId, bool enableInterception)
    {
        if (_remoteConfigurations.TryGetValue(deviceId, out var config))
        {
            config.EnableInterception = enableInterception;
            _logger.LogInformation("Set button interception for {DeviceId} to {Enabled}", deviceId, enableInterception);
        }
    }

    private async Task ProcessLongPressAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && _isListening)
        {
            try
            {
                await Task.Delay(100, cancellationToken); // Check every 100ms

                var now = DateTime.UtcNow;
                foreach (var state in _buttonStates.Values.Where(s => s.IsPressed))
                {
                    var config = _remoteConfigurations.GetValueOrDefault(state.DeviceId) ?? new RemoteConfiguration();
                    var holdDuration = now - state.PressStartTime;

                    if (!state.LongPressFired && holdDuration.TotalMilliseconds >= config.LongPressTimeoutMs)
                    {
                        state.LongPressFired = true;

                        var eventArgs = new RemoteButtonEventArgs(
                            state.DeviceId, state.ButtonName, state.KeyCode,
                            ButtonEventType.LongPress,
                            holdDuration: holdDuration);

                        _logger.LogDebug("Long press detected on {DeviceId}: {ButtonName} (0x{KeyCode:X2})",
                            state.DeviceId, state.ButtonName, state.KeyCode);

                        ButtonLongPress?.Invoke(this, eventArgs);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error during long press processing");
            }
        }
    }

    private bool IsSystemButton(byte keyCode)
    {
        // Define system buttons that should be interceptable
        return keyCode switch
        {
            0x01 => true, // Power
            0x7F => true, // Sleep
            0x80 => true, // Wake
            _ => false
        };
    }

    private class ButtonState
    {
        public string DeviceId { get; set; } = "";
        public byte KeyCode { get; set; }
        public string ButtonName { get; set; } = "";
        public bool IsPressed { get; set; }
        public DateTime PressStartTime { get; set; }
        public DateTime LastRepeatTime { get; set; }
        public bool LongPressFired { get; set; }
    }

    private class RemoteConfiguration
    {
        public int LongPressTimeoutMs { get; set; } = 500;
        public bool EnableInterception { get; set; } = false;
        public int RepeatDelayMs { get; set; } = 500;
        public int RepeatRateMs { get; set; } = 100;
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