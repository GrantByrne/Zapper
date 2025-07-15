namespace Zapper.Device.Network;

public interface INetworkDeviceController
{
    Task<bool> SendCommand(string ipAddress, int port, string command, string? payload = null, CancellationToken cancellationToken = default);
    Task<bool> SendHttpCommand(string baseUrl, string endpoint, string method = "POST", string? payload = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);
    Task<bool> SendWebSocketCommand(string wsUrl, string command, CancellationToken cancellationToken = default);
    Task<string?> DiscoverDevices(string deviceType, TimeSpan timeout, CancellationToken cancellationToken = default);
}