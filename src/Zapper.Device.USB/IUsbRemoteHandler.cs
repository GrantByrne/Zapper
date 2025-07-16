namespace Zapper.Device.USB;

public interface IUsbRemoteHandler
{
    event EventHandler<RemoteButtonEventArgs>? ButtonPressed;
    event EventHandler<RemoteButtonEventArgs>? ButtonDown;
    event EventHandler<RemoteButtonEventArgs>? ButtonUp;
    event EventHandler<RemoteButtonEventArgs>? ButtonLongPress;
    event EventHandler<string>? RemoteConnected;
    event EventHandler<string>? RemoteDisconnected;

    Task StartListening(CancellationToken cancellationToken = default);
    Task StopListening();
    bool IsListening { get; }
    IEnumerable<string> GetConnectedRemotes();
    void ConfigureLongPressTimeout(string deviceId, int timeoutMs);
    void ConfigureButtonInterception(string deviceId, bool enableInterception);
}