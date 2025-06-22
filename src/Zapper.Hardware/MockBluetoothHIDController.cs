using Microsoft.Extensions.Logging;

namespace Zapper.Hardware;

public class MockBluetoothHIDController : IBluetoothHIDController
{
    private readonly ILogger<MockBluetoothHIDController> _logger;
    private bool _isAdvertising;
    private bool _isConnected;
    private string? _connectedDeviceId;

    public bool IsConnected => _isConnected;
    public bool IsAdvertising => _isAdvertising;
    public string? ConnectedDeviceId => _connectedDeviceId;

    public event EventHandler<string>? DeviceConnected;
    public event EventHandler<string>? DeviceDisconnected;

    public MockBluetoothHIDController(ILogger<MockBluetoothHIDController> logger)
    {
        _logger = logger;
    }

    public Task<bool> StartAdvertisingAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Mock: Starting Bluetooth HID advertising");
        _isAdvertising = true;
        return Task.FromResult(true);
    }

    public Task<bool> StopAdvertisingAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Mock: Stopping Bluetooth HID advertising");
        _isAdvertising = false;
        return Task.FromResult(true);
    }

    public Task<bool> ConnectToDeviceAsync(string deviceId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Mock: Connecting to Bluetooth device {DeviceId}", deviceId);
        _isConnected = true;
        _connectedDeviceId = deviceId;
        DeviceConnected?.Invoke(this, deviceId);
        return Task.FromResult(true);
    }

    public Task<bool> DisconnectAsync(CancellationToken cancellationToken = default)
    {
        if (_isConnected && _connectedDeviceId != null)
        {
            _logger.LogInformation("Mock: Disconnecting from Bluetooth device {DeviceId}", _connectedDeviceId);
            var deviceId = _connectedDeviceId;
            _isConnected = false;
            _connectedDeviceId = null;
            DeviceDisconnected?.Invoke(this, deviceId);
        }
        return Task.FromResult(true);
    }

    public Task<bool> SendKeyEventAsync(HIDKeyCode keyCode, bool isPressed = true, CancellationToken cancellationToken = default)
    {
        if (!_isConnected)
        {
            _logger.LogWarning("Mock: Cannot send key event - not connected to any device");
            return Task.FromResult(false);
        }

        _logger.LogInformation("Mock: Sending key event {KeyCode} (pressed: {IsPressed}) to device {DeviceId}", 
            keyCode, isPressed, _connectedDeviceId);
        return Task.FromResult(true);
    }

    public Task<bool> SendMouseEventAsync(int deltaX, int deltaY, bool leftClick = false, bool rightClick = false, CancellationToken cancellationToken = default)
    {
        if (!_isConnected)
        {
            _logger.LogWarning("Mock: Cannot send mouse event - not connected to any device");
            return Task.FromResult(false);
        }

        _logger.LogInformation("Mock: Sending mouse event (deltaX: {DeltaX}, deltaY: {DeltaY}, leftClick: {LeftClick}, rightClick: {RightClick}) to device {DeviceId}", 
            deltaX, deltaY, leftClick, rightClick, _connectedDeviceId);
        return Task.FromResult(true);
    }

    public Task<bool> SendKeyboardTextAsync(string text, CancellationToken cancellationToken = default)
    {
        if (!_isConnected)
        {
            _logger.LogWarning("Mock: Cannot send keyboard text - not connected to any device");
            return Task.FromResult(false);
        }

        _logger.LogInformation("Mock: Sending keyboard text '{Text}' to device {DeviceId}", text, _connectedDeviceId);
        return Task.FromResult(true);
    }

    public Task<IEnumerable<string>> GetPairedDevicesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Mock: Getting paired Bluetooth devices");
        var mockDevices = new[]
        {
            "Mock Android TV (AA:BB:CC:DD:EE:FF)",
            "Mock Apple TV (11:22:33:44:55:66)",
            "Mock Smart TV (99:88:77:66:55:44)"
        };
        return Task.FromResult<IEnumerable<string>>(mockDevices);
    }
}