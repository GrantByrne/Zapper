namespace Zapper.Device.Xbox.Network;

public class NetworkClientFactory : INetworkClientFactory
{
    public ITcpClientWrapper CreateTcpClient()
    {
        return new TcpClientWrapper();
    }

    public IUdpClientWrapper CreateUdpClient()
    {
        return new UdpClientWrapper();
    }
}