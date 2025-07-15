using Microsoft.Extensions.Logging;

namespace Zapper.Device.USB;

public class MockUsbRemoteHandler(ILogger<MockUsbRemoteHandler> logger) : IUsbRemoteHandler
{
    private bool _isListening;
    private readonly Random _random = new();

    public event EventHandler<RemoteButtonEventArgs>? ButtonPressed;

    public bool IsListening => _isListening;

    public Task StartListening(CancellationToken cancellationToken = default)
    {
        if (_isListening)
            return Task.CompletedTask;

        _isListening = true;
        logger.LogInformation("Mock USB remote handler started");

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
        return ["MOCK:0001:remote1", "MOCK:0002:remote2"];
    }

    public void SimulateButtonPress(string deviceId, string buttonName, int keyCode = 0)
    {
        if (!_isListening)
            return;

        var eventArgs = new RemoteButtonEventArgs(deviceId, buttonName, keyCode);
        logger.LogDebug("Simulating button press: {DeviceId} - {ButtonName}", deviceId, buttonName);
        ButtonPressed?.Invoke(this, eventArgs);
    }

    private async Task SimulateButtonPressesAsync(CancellationToken cancellationToken)
    {
        var buttons = new[] { "Power", "VolumeUp", "VolumeDown", "ChannelUp", "ChannelDown", "OK", "Menu" };
        var devices = GetConnectedRemotes().ToArray();

        while (!cancellationToken.IsCancellationRequested && _isListening)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(10 + _random.Next(20)), cancellationToken);

                if (_isListening)
                {
                    var device = devices[_random.Next(devices.Length)];
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