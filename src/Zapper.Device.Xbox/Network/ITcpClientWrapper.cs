namespace Zapper.Device.Xbox.Network;

public interface ITcpClientWrapper : IDisposable
{
    Task Connect(string hostname, int port);
    Stream GetStream();
}