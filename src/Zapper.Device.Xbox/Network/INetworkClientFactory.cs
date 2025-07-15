namespace Zapper.Device.Xbox.Network;

public interface INetworkClientFactory
{
    ITcpClientWrapper CreateTcpClient();
    IUdpClientWrapper CreateUdpClient();
}