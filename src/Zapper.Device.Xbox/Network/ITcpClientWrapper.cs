namespace Zapper.Device.Xbox.Network;

public interface ITcpClientWrapper : IDisposable
{
    Task ConnectAsync(string hostname, int port);
    Stream GetStream();
}