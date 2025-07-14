using Microsoft.Extensions.Logging;

namespace Zapper.Device.Bluetooth;

public class BluetoothHidController : IBluetoothHIDController
{
    private readonly IBluetoothService _bluetoothService;
    private readonly ILogger<BluetoothHidController> _logger;
    private readonly Dictionary<string, BluetoothDeviceInfo> _connectedDevices = new();

    public BluetoothHidController(IBluetoothService bluetoothService, ILogger<BluetoothHidController> logger)
    {
        _bluetoothService = bluetoothService ?? throw new ArgumentNullException(nameof(bluetoothService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _bluetoothService.DeviceConnected += OnDeviceConnected;
        _bluetoothService.DeviceDisconnected += OnDeviceDisconnected;
    }

    public Task<bool> SendKeyAsync(string deviceAddress, HIDKeyCode keyCode, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_connectedDevices.ContainsKey(deviceAddress))
            {
                _logger.LogWarning("Device {Address} is not connected", deviceAddress);
                return Task.FromResult(false);
            }

            _logger.LogDebug("Sending HID key {KeyCode} to device {Address}", keyCode, deviceAddress);

            var device = _connectedDevices[deviceAddress];
            if (IsHidDevice(device))
            {
                _logger.LogInformation("Simulating HID key {KeyCode} sent to device {Address} ({Name})", 
                    keyCode, deviceAddress, device.Name);
                return Task.FromResult(true);
            }
            else
            {
                _logger.LogWarning("Device {Address} ({Name}) does not support HID profile", 
                    deviceAddress, device.Name);
                return Task.FromResult(false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send HID key {KeyCode} to device {Address}", keyCode, deviceAddress);
            return Task.FromResult(false);
        }
    }

    public async Task<bool> SendKeySequenceAsync(string deviceAddress, HIDKeyCode[] keyCodes, int delayMs = 50, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_connectedDevices.ContainsKey(deviceAddress))
            {
                _logger.LogWarning("Device {Address} is not connected", deviceAddress);
                return false;
            }

            _logger.LogDebug("Sending HID key sequence to device {Address}: {Keys}", 
                deviceAddress, string.Join(", ", keyCodes));

            foreach (var keyCode in keyCodes)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                if (!await SendKeyAsync(deviceAddress, keyCode, cancellationToken))
                {
                    _logger.LogWarning("Failed to send key {KeyCode} in sequence to device {Address}", 
                        keyCode, deviceAddress);
                    return false;
                }

                if (delayMs > 0)
                {
                    await Task.Delay(delayMs, cancellationToken);
                }
            }

            return true;
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Key sequence sending was cancelled for device {Address}", deviceAddress);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send key sequence to device {Address}", deviceAddress);
            return false;
        }
    }

    public async Task<bool> SendTextAsync(string deviceAddress, string text, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(text))
                return true;

            if (!_connectedDevices.ContainsKey(deviceAddress))
            {
                _logger.LogWarning("Device {Address} is not connected", deviceAddress);
                return false;
            }

            _logger.LogDebug("Sending text '{Text}' to device {Address}", text, deviceAddress);

            var keyCodes = new List<HIDKeyCode>();
            foreach (char c in text)
            {
                var keyCode = CharToHidKeyCode(c);
                if (keyCode != HIDKeyCode.None)
                {
                    keyCodes.Add(keyCode);
                }
                else
                {
                    _logger.LogWarning("Cannot convert character '{Char}' to HID key code", c);
                }
            }

            return await SendKeySequenceAsync(deviceAddress, keyCodes.ToArray(), 50, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send text to device {Address}", deviceAddress);
            return false;
        }
    }

    public async Task<bool> ConnectAsync(string deviceAddress, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Connecting to HID device {Address}...", deviceAddress);
            return await _bluetoothService.ConnectDeviceAsync(deviceAddress, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to HID device {Address}", deviceAddress);
            return false;
        }
    }

    public async Task<bool> DisconnectAsync(string deviceAddress, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Disconnecting from HID device {Address}...", deviceAddress);
            return await _bluetoothService.DisconnectDeviceAsync(deviceAddress, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to disconnect from HID device {Address}", deviceAddress);
            return false;
        }
    }

    public async Task<bool> IsConnectedAsync(string deviceAddress, CancellationToken cancellationToken = default)
    {
        try
        {
            var device = await _bluetoothService.GetDeviceAsync(deviceAddress, cancellationToken);
            return device?.IsConnected ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check connection status for device {Address}", deviceAddress);
            return false;
        }
    }

    public async Task<IEnumerable<string>> GetConnectedDevicesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var devices = await _bluetoothService.GetDevicesAsync(cancellationToken);
            return devices
                .Where(d => d.IsConnected && IsHidDevice(d))
                .Select(d => d.Address)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get connected HID devices");
            return [];
        }
    }

    private void OnDeviceConnected(object? sender, BluetoothDeviceEventArgs e)
    {
        if (IsHidDevice(e.Device))
        {
            _connectedDevices[e.Device.Address] = e.Device;
            _logger.LogInformation("HID device connected: {Name} ({Address})", e.Device.Name, e.Device.Address);
        }
    }

    private void OnDeviceDisconnected(object? sender, BluetoothDeviceEventArgs e)
    {
        if (_connectedDevices.ContainsKey(e.Device.Address))
        {
            _connectedDevices.Remove(e.Device.Address);
            _logger.LogInformation("HID device disconnected: {Name} ({Address})", e.Device.Name, e.Device.Address);
        }
    }

    private static bool IsHidDevice(BluetoothDeviceInfo device)
    {
        const string hidServiceUuid = "00001124-0000-1000-8000-00805f9b34fb";
        return device.UUIDs.Any(uuid => string.Equals(uuid, hidServiceUuid, StringComparison.OrdinalIgnoreCase));
    }

    private static HIDKeyCode CharToHidKeyCode(char c)
    {
        return c switch
        {
            ' ' => HIDKeyCode.Space,
            '0' => HIDKeyCode.Key0,
            '1' => HIDKeyCode.Key1,
            '2' => HIDKeyCode.Key2,
            '3' => HIDKeyCode.Key3,
            '4' => HIDKeyCode.Key4,
            '5' => HIDKeyCode.Key5,
            '6' => HIDKeyCode.Key6,
            '7' => HIDKeyCode.Key7,
            '8' => HIDKeyCode.Key8,
            '9' => HIDKeyCode.Key9,
            'a' or 'A' => HIDKeyCode.A,
            'b' or 'B' => HIDKeyCode.B,
            'c' or 'C' => HIDKeyCode.C,
            'd' or 'D' => HIDKeyCode.D,
            'e' or 'E' => HIDKeyCode.E,
            'f' or 'F' => HIDKeyCode.F,
            'g' or 'G' => HIDKeyCode.G,
            'h' or 'H' => HIDKeyCode.H,
            'i' or 'I' => HIDKeyCode.I,
            'j' or 'J' => HIDKeyCode.J,
            'k' or 'K' => HIDKeyCode.K,
            'l' or 'L' => HIDKeyCode.L,
            'm' or 'M' => HIDKeyCode.M,
            'n' or 'N' => HIDKeyCode.N,
            'o' or 'O' => HIDKeyCode.O,
            'p' or 'P' => HIDKeyCode.P,
            'q' or 'Q' => HIDKeyCode.Q,
            'r' or 'R' => HIDKeyCode.R,
            's' or 'S' => HIDKeyCode.S,
            't' or 'T' => HIDKeyCode.T,
            'u' or 'U' => HIDKeyCode.U,
            'v' or 'V' => HIDKeyCode.V,
            'w' or 'W' => HIDKeyCode.W,
            'x' or 'X' => HIDKeyCode.X,
            'y' or 'Y' => HIDKeyCode.Y,
            'z' or 'Z' => HIDKeyCode.Z,
            '\n' => HIDKeyCode.Enter,
            '\r' => HIDKeyCode.Enter,
            '\t' => HIDKeyCode.Tab,
            '\b' => HIDKeyCode.Backspace,
            _ => HIDKeyCode.None
        };
    }
}