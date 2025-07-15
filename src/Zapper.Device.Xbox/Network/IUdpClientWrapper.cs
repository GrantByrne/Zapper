using System.Net;
using System.Net.Sockets;

namespace Zapper.Device.Xbox.Network;

public interface IUdpClientWrapper : IDisposable
{
    bool EnableBroadcast { get; set; }
    Socket Client { get; }
    Task<int> SendAsync(byte[] dgram, int bytes, IPEndPoint? endPoint);
    Task<UdpReceiveResult> ReceiveAsync(CancellationToken cancellationToken);
}