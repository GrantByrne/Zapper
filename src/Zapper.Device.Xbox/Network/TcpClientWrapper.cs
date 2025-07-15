using System.Net.Sockets;

namespace Zapper.Device.Xbox.Network;

public class TcpClientWrapper : ITcpClientWrapper
{
    private readonly TcpClient _tcpClient;

    public TcpClientWrapper()
    {
        _tcpClient = new TcpClient();
    }

    public Task ConnectAsync(string hostname, int port)
    {
        return _tcpClient.ConnectAsync(hostname, port);
    }

    public Stream GetStream()
    {
        return _tcpClient.GetStream();
    }

    public void Dispose()
    {
        _tcpClient?.Dispose();
    }
}