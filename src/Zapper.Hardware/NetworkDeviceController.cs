using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Zapper.Hardware;

public class NetworkDeviceController : INetworkDeviceController
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NetworkDeviceController> _logger;

    public NetworkDeviceController(HttpClient httpClient, ILogger<NetworkDeviceController> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<bool> SendCommandAsync(string ipAddress, int port, string command, string? payload = null, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = new TcpClient();
            await client.ConnectAsync(IPAddress.Parse(ipAddress), port, cancellationToken);
            
            using var stream = client.GetStream();
            var commandData = Encoding.UTF8.GetBytes(command + (payload ?? ""));
            
            await stream.WriteAsync(commandData, cancellationToken);
            await stream.FlushAsync(cancellationToken);
            
            _logger.LogDebug("Sent TCP command to {IpAddress}:{Port}: {Command}", ipAddress, port, command);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send TCP command to {IpAddress}:{Port}", ipAddress, port);
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
            
            var response = await _httpClient.SendAsync(request, cancellationToken);
            var success = response.IsSuccessStatusCode;
            
            _logger.LogDebug("Sent HTTP {Method} to {Url}: {Success}", method, request.RequestUri, success);
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send HTTP command to {BaseUrl}/{Endpoint}", baseUrl, endpoint);
            return false;
        }
    }

    public async Task<bool> SendWebSocketCommandAsync(string wsUrl, string command, CancellationToken cancellationToken = default)
    {
        try
        {
            // WebSocket implementation would go here
            // For now, just log the attempt
            _logger.LogDebug("WebSocket command to {WsUrl}: {Command}", wsUrl, command);
            await Task.Delay(100, cancellationToken); // Simulate network delay
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send WebSocket command to {WsUrl}", wsUrl);
            return false;
        }
    }

    public async Task<string?> DiscoverDevicesAsync(string deviceType, TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting device discovery for {DeviceType}", deviceType);
            
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
            
            _logger.LogDebug("Sent SSDP discovery message for {DeviceType}", deviceType);
            
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
                    
                    if (completedTask == receiveTask)
                    {
                        var result = await receiveTask;
                        var response = Encoding.UTF8.GetString(result.Buffer);
                        discoveredDevices.Add($"{result.RemoteEndPoint}: {response}");
                        _logger.LogDebug("Received discovery response from {Endpoint}", result.RemoteEndPoint);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error during device discovery");
                }
            }
            
            _logger.LogInformation("Device discovery completed. Found {Count} devices", discoveredDevices.Count);
            return discoveredDevices.Count > 0 ? JsonSerializer.Serialize(discoveredDevices) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to discover devices of type {DeviceType}", deviceType);
            return null;
        }
    }
}