using Microsoft.Extensions.Logging;

namespace Zapper.Device.Bluetooth;

public class MockBluetoothHidController(ILogger<MockBluetoothHidController> logger) : IBluetoothHidController
{
    private readonly ILogger<MockBluetoothHidController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly Dictionary<string, bool> _connectedDevices = new();

    public Task<bool> SendKey(string deviceAddress, HidKeyCode keyCode, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(deviceAddress))
        {
            _logger.LogWarning("Mock: Cannot send key - device address is null or empty");
            return Task.FromResult(false);
        }

        _logger.LogInformation("Mock: Sending key {KeyCode} to device {DeviceAddress}", keyCode, deviceAddress);
        return Task.FromResult(true);
    }

    public Task<bool> SendKeySequence(string deviceAddress, HidKeyCode[] keyCodes, int delayMs = 50, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(deviceAddress))
        {
            _logger.LogWarning("Mock: Cannot send key sequence - device address is null or empty");
            return Task.FromResult(false);
        }

        _logger.LogInformation("Mock: Sending key sequence [{KeyCodes}] to device {DeviceAddress} with {DelayMs}ms delay",
            string.Join(", ", keyCodes), deviceAddress, delayMs);
        return Task.FromResult(true);
    }

    public Task<bool> SendText(string deviceAddress, string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(deviceAddress))
        {
            _logger.LogWarning("Mock: Cannot send text - device address is null or empty");
            return Task.FromResult(false);
        }

        _logger.LogInformation("Mock: Sending text '{Text}' to device {DeviceAddress}", text, deviceAddress);
        return Task.FromResult(true);
    }

    public Task<bool> Connect(string deviceAddress, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(deviceAddress))
        {
            _logger.LogWarning("Mock: Cannot connect - device address is null or empty");
            return Task.FromResult(false);
        }

        _logger.LogInformation("Mock: Connecting to device {DeviceAddress}", deviceAddress);
        _connectedDevices[deviceAddress] = true;
        return Task.FromResult(true);
    }

    public Task<bool> Disconnect(string deviceAddress, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(deviceAddress))
        {
            _logger.LogWarning("Mock: Cannot disconnect - device address is null or empty");
            return Task.FromResult(false);
        }

        _logger.LogInformation("Mock: Disconnecting from device {DeviceAddress}", deviceAddress);
        _connectedDevices[deviceAddress] = false;
        return Task.FromResult(true);
    }

    public Task<bool> IsConnected(string deviceAddress, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(deviceAddress))
        {
            return Task.FromResult(false);
        }

        var isConnected = _connectedDevices.GetValueOrDefault(deviceAddress, false);
        _logger.LogInformation("Mock: Device {DeviceAddress} connection status: {IsConnected}", deviceAddress, isConnected);
        return Task.FromResult(isConnected);
    }

    public Task<IEnumerable<string>> GetConnectedDevices(CancellationToken cancellationToken = default)
    {
        var connectedDevices = _connectedDevices
            .Where(kvp => kvp.Value)
            .Select(kvp => kvp.Key)
            .ToList();

        _logger.LogInformation("Mock: Getting connected devices: [{Devices}]", string.Join(", ", connectedDevices));
        return Task.FromResult<IEnumerable<string>>(connectedDevices);
    }
}