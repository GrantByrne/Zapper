namespace Zapper.Device.USB;

public interface IUsbRemoteHandler
{
    event EventHandler<RemoteButtonEventArgs>? ButtonPressed;
    Task StartListeningAsync(CancellationToken cancellationToken = default);
    Task StopListeningAsync();
    bool IsListening { get; }
    IEnumerable<string> GetConnectedRemotes();
}