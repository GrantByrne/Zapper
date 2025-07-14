namespace Zapper.Device.Network;

public interface INetworkDeviceController
{
    Task<bool> SendCommandAsync(string ipAddress, int port, string command, string? payload = null, CancellationToken cancellationToken = default);
    Task<bool> SendHttpCommandAsync(string baseUrl, string endpoint, string method = "POST", string? payload = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);
    Task<bool> SendWebSocketCommandAsync(string wsUrl, string command, CancellationToken cancellationToken = default);
    Task<string?> DiscoverDevicesAsync(string deviceType, TimeSpan timeout, CancellationToken cancellationToken = default);
}