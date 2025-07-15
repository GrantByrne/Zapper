using System.Net;
using System.Net.Sockets;

namespace Zapper.Device.Xbox.Network;

public class UdpClientWrapper : IUdpClientWrapper
{
    private readonly UdpClient _udpClient;

    public UdpClientWrapper()
    {
        _udpClient = new UdpClient();
    }

    public bool EnableBroadcast
    {
        get => _udpClient.EnableBroadcast;
        set => _udpClient.EnableBroadcast = value;
    }

    public Socket Client => _udpClient.Client;

    public Task<int> SendAsync(byte[] dgram, int bytes, IPEndPoint? endPoint)
    {
        return _udpClient.SendAsync(dgram, bytes, endPoint);
    }

    public Task<UdpReceiveResult> ReceiveAsync(CancellationToken cancellationToken)
    {
        return _udpClient.ReceiveAsync(cancellationToken).AsTask();
    }

    public void Dispose()
    {
        _udpClient?.Dispose();
    }
}