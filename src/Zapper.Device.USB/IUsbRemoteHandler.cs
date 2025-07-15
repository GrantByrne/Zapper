namespace Zapper.Device.USB;

public interface IUsbRemoteHandler
{
    event EventHandler<RemoteButtonEventArgs>? ButtonPressed;
    Task StartListening(CancellationToken cancellationToken = default);
    Task StopListening();
    bool IsListening { get; }
    IEnumerable<string> GetConnectedRemotes();
}