using Microsoft.Extensions.Logging;
using Zapper.Core.Models;

namespace Zapper.Device.USB;

public class MockUsbRemoteHandler(ILogger<MockUsbRemoteHandler> logger) : IUsbRemoteHandler
{
    private bool _isListening;
    private readonly Random _random = new();
    private readonly List<string> _connectedDevices = new();

    public event EventHandler<RemoteButtonEventArgs>? ButtonPressed;
    public event EventHandler<RemoteButtonEventArgs>? ButtonDown;
    public event EventHandler<RemoteButtonEventArgs>? ButtonUp;
#pragma warning disable CS0067 // The event is never used
    public event EventHandler<RemoteButtonEventArgs>? ButtonLongPress;
    public event EventHandler<string>? RemoteDisconnected;
#pragma warning restore CS0067
    public event EventHandler<string>? RemoteConnected;

    public bool IsListening => _isListening;

    public Task StartListening(CancellationToken cancellationToken = default)
    {
        if (_isListening)
            return Task.CompletedTask;

        _isListening = true;
        logger.LogInformation("Mock USB remote handler started");

        // Simulate some connected devices
        SimulateDeviceConnections();

        // Optionally simulate random button presses for testing
        if (logger.IsEnabled(LogLevel.Debug))
        {
            _ = Task.Run(() => SimulateButtonPressesAsync(cancellationToken), cancellationToken);
        }

        return Task.CompletedTask;
    }

    public Task StopListening()
    {
        _isListening = false;
        logger.LogInformation("Mock USB remote handler stopped");
        return Task.CompletedTask;
    }

    public IEnumerable<string> GetConnectedRemotes()
    {
        return _connectedDevices.ToList();
    }

    public void SimulateButtonPress(string deviceId, string buttonName, int keyCode = 0)
    {
        if (!_isListening)
            return;

        var eventArgs = new RemoteButtonEventArgs(deviceId, buttonName, keyCode);
        logger.LogDebug("Simulating button press: {DeviceId} - {ButtonName}", deviceId, buttonName);

        // Simulate full button lifecycle
        ButtonDown?.Invoke(this, new RemoteButtonEventArgs(deviceId, buttonName, keyCode, ButtonEventType.KeyDown));
        ButtonPressed?.Invoke(this, eventArgs);
        ButtonUp?.Invoke(this, new RemoteButtonEventArgs(deviceId, buttonName, keyCode, ButtonEventType.KeyUp));
    }

    private void SimulateDeviceConnections()
    {
        var mockDevices = new[]
        {
            "046D:C52B:LogitechK380",  // Logitech K380 Keyboard
            "046D:C534:UnifyingReceiver", // Logitech Unifying Receiver
            "05AC:0267:AppleKeyboard",  // Apple Magic Keyboard
            "045E:07A5:MicrosoftKeyboard", // Microsoft Keyboard
            "FIDO:0001:FIDOAlliance", // FIDO/U2F Security Key
            "0C45:7603:GenericUSBKeyboard" // Generic USB Keyboard
        };

        foreach (var device in mockDevices)
        {
            _connectedDevices.Add(device);
            logger.LogInformation("Mock device connected: {DeviceId}", device);
            RemoteConnected?.Invoke(this, device);
        }
    }

    public void ConfigureLongPressTimeout(string deviceId, int timeoutMs)
    {
        logger.LogInformation("Mock: Configured long press timeout for {DeviceId} to {Timeout}ms", deviceId, timeoutMs);
    }

    public void ConfigureButtonInterception(string deviceId, bool enableInterception)
    {
        logger.LogInformation("Mock: Configured button interception for {DeviceId} to {Enabled}", deviceId, enableInterception);
    }

    private async Task SimulateButtonPressesAsync(CancellationToken cancellationToken)
    {
        var buttons = new[] { "Power", "VolumeUp", "VolumeDown", "ChannelUp", "ChannelDown", "OK", "Menu",
                              "A", "B", "Enter", "Space", "Escape" }; // Added keyboard keys

        while (!cancellationToken.IsCancellationRequested && _isListening)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(10 + _random.Next(20)), cancellationToken);

                if (_isListening && _connectedDevices.Count > 0)
                {
                    var device = _connectedDevices[_random.Next(_connectedDevices.Count)];
                    var button = buttons[_random.Next(buttons.Length)];
                    SimulateButtonPress(device, button, _random.Next(1, 255));
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}