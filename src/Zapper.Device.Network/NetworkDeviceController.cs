using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Zapper.Device.Network;

public class NetworkDeviceController(HttpClient httpClient, ILogger<NetworkDeviceController> logger) : INetworkDeviceController, IDisposable
{
    private readonly ConcurrentDictionary<string, ClientWebSocket> _webSocketConnections = new();
    private bool _disposed;

    public async Task<bool> SendCommandAsync(string ipAddress, int port, string command, string? payload = null, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = new TcpClient();
            await client.ConnectAsync(IPAddress.Parse(ipAddress), port, cancellationToken);

            await using var stream = client.GetStream();
            var commandData = Encoding.UTF8.GetBytes(command + (payload ?? ""));

            await stream.WriteAsync(commandData, cancellationToken);
            await stream.FlushAsync(cancellationToken);

            logger.LogDebug("Sent TCP command to {IpAddress}:{Port}: {Command}", ipAddress, port, command);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send TCP command to {IpAddress}:{Port}", ipAddress, port);
            return false;
        }
    }

    public async Task<bool> SendHttpCommandAsync(string baseUrl, string endpoint, string method = "POST", string? payload = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new HttpRequestMessage(new HttpMethod(method), $"{baseUrl.TrimEnd('/')}/{endpoint.TrimStart('/')}");

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            if (!string.IsNullOrEmpty(payload))
            {
                request.Content = new StringContent(payload, Encoding.UTF8, "application/json");
            }

            var response = await httpClient.SendAsync(request, cancellationToken);
            var success = response.IsSuccessStatusCode;

            logger.LogDebug("Sent HTTP {Method} to {Url}: {Success}", method, request.RequestUri, success);
            return success;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send HTTP command to {BaseUrl}/{Endpoint}", baseUrl, endpoint);
            return false;
        }
    }

    public async Task<bool> SendWebSocketCommandAsync(string wsUrl, string command, CancellationToken cancellationToken = default)
    {
        try
        {
            var webSocket = await GetOrCreateWebSocketAsync(wsUrl, cancellationToken);
            if (webSocket?.State != WebSocketState.Open)
            {
                logger.LogWarning("WebSocket connection to {WsUrl} is not open", wsUrl);
                return false;
            }

            var commandBytes = Encoding.UTF8.GetBytes(command);
            await webSocket.SendAsync(
                new ArraySegment<byte>(commandBytes),
                WebSocketMessageType.Text,
                true,
                cancellationToken);

            logger.LogDebug("Sent WebSocket command to {WsUrl}: {Command}", wsUrl, command);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send WebSocket command to {WsUrl}", wsUrl);

            // Clean up failed connection
            if (_webSocketConnections.TryRemove(wsUrl, out var failedSocket))
            {
                failedSocket.Dispose();
            }

            return false;
        }
    }

    private async Task<ClientWebSocket?> GetOrCreateWebSocketAsync(string wsUrl, CancellationToken cancellationToken)
    {
        if (_webSocketConnections.TryGetValue(wsUrl, out var existingSocket) &&
            existingSocket.State == WebSocketState.Open)
        {
            return existingSocket;
        }

        // Remove any existing closed connections
        if (existingSocket != null)
        {
            _webSocketConnections.TryRemove(wsUrl, out _);
            existingSocket.Dispose();
        }

        try
        {
            var newSocket = new ClientWebSocket();
            await newSocket.ConnectAsync(new Uri(wsUrl), cancellationToken);

            if (_webSocketConnections.TryAdd(wsUrl, newSocket))
            {
                logger.LogDebug("Established WebSocket connection to {WsUrl}", wsUrl);
                return newSocket;
            }

            // Another thread created the connection
            newSocket.Dispose();
            return _webSocketConnections.TryGetValue(wsUrl, out var socket) ? socket : null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to establish WebSocket connection to {WsUrl}", wsUrl);
            return null;
        }
    }

    public async Task<string?> DiscoverDevicesAsync(string deviceType, TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Starting device discovery for {DeviceType}", deviceType);

            // Simple SSDP-style discovery
            using var client = new UdpClient();
            var multicastEndpoint = new IPEndPoint(IPAddress.Parse("239.255.255.250"), 1900);

            var searchMessage = $"M-SEARCH * HTTP/1.1\r\n" +
                               $"HOST: 239.255.255.250:1900\r\n" +
                               $"MAN: \"ssdp:discover\"\r\n" +
                               $"ST: {deviceType}\r\n" +
                               $"MX: 3\r\n\r\n";

            var searchBytes = Encoding.UTF8.GetBytes(searchMessage);
            await client.SendAsync(searchBytes, multicastEndpoint, cancellationToken);

            logger.LogDebug("Sent SSDP discovery message for {DeviceType}", deviceType);

            // Wait for responses
            var endTime = DateTime.UtcNow.Add(timeout);
            var discoveredDevices = new List<string>();

            while (DateTime.UtcNow < endTime && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var receiveTask = client.ReceiveAsync();
                    var timeoutTask = Task.Delay(1000, cancellationToken);

                    var completedTask = await Task.WhenAny(receiveTask, timeoutTask);

                    if (completedTask != receiveTask)
                        continue;

                    var result = await receiveTask;
                    var response = Encoding.UTF8.GetString(result.Buffer);
                    discoveredDevices.Add($"{result.RemoteEndPoint}: {response}");
                    logger.LogDebug("Received discovery response from {Endpoint}", result.RemoteEndPoint);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Error during device discovery");
                }
            }

            logger.LogInformation("Device discovery completed. Found {Count} devices", discoveredDevices.Count);
            return discoveredDevices.Count > 0 ? JsonSerializer.Serialize(discoveredDevices) : null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to discover devices of type {DeviceType}", deviceType);
            return null;
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        foreach (var webSocket in _webSocketConnections.Values)
        {
            try
            {
                webSocket.Dispose();
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error disposing WebSocket connection");
            }
        }

        _webSocketConnections.Clear();
        _disposed = true;
    }
}