using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Zapper.Device.Xbox.Models;

namespace Zapper.Device.Xbox;

public class XboxDiscovery(ILogger<XboxDiscovery> logger) : IXboxDiscovery
{
    private const int DiscoveryPort = 5050;
    private const string DiscoveryMessage = "{\"type\":\"discovery\",\"version\":2}";
    
    public event EventHandler<XboxDevice>? DeviceFound;

    public async Task<IEnumerable<XboxDevice>> DiscoverDevicesAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        var devices = new List<XboxDevice>();
        var discoveredDevices = new HashSet<string>();

        try
        {
            using var udpClient = new UdpClient();
            udpClient.EnableBroadcast = true;
            
            var broadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, DiscoveryPort);
            var message = Encoding.UTF8.GetBytes(DiscoveryMessage);
            
            logger.LogInformation("Starting Xbox console discovery for {Timeout} seconds", timeout.TotalSeconds);
            
            await udpClient.SendAsync(message, message.Length, broadcastEndpoint);
            
            var endTime = DateTime.UtcNow.Add(timeout);
            udpClient.Client.ReceiveTimeout = 1000;
            
            while (DateTime.UtcNow < endTime && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var result = await ReceiveWithTimeoutAsync(udpClient, 1000, cancellationToken);
                    if (result == null) continue;
                    
                    var response = Encoding.UTF8.GetString(result.Value.Buffer);
                    var device = ParseDiscoveryResponse(response, result.Value.RemoteEndPoint.Address.ToString());
                    
                    if (device != null && discoveredDevices.Add(device.IpAddress))
                    {
                        devices.Add(device);
                        DeviceFound?.Invoke(this, device);
                        logger.LogInformation("Discovered Xbox console: {Name} at {IpAddress}", device.Name, device.IpAddress);
                    }
                }
                catch (SocketException)
                {
                    // Timeout, continue
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Error processing discovery response");
                }
            }
            
            logger.LogInformation("Xbox discovery completed. Found {Count} consoles", devices.Count);
            return devices;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to discover Xbox consoles");
            return devices;
        }
    }

    private async Task<UdpReceiveResult?> ReceiveWithTimeoutAsync(UdpClient client, int timeoutMs, CancellationToken cancellationToken)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(timeoutMs);
        
        try
        {
            return await client.ReceiveAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            return null;
        }
    }

    private XboxDevice? ParseDiscoveryResponse(string response, string ipAddress)
    {
        try
        {
            using var doc = JsonDocument.Parse(response);
            var root = doc.RootElement;
            
            if (!root.TryGetProperty("type", out var typeElement) || typeElement.GetString() != "device")
                return null;
            
            var device = new XboxDevice
            {
                IpAddress = ipAddress,
                LastSeen = DateTime.UtcNow
            };
            
            if (root.TryGetProperty("name", out var nameElement))
                device.Name = nameElement.GetString() ?? "Xbox Console";
            
            if (root.TryGetProperty("id", out var idElement))
                device.LiveId = idElement.GetString() ?? string.Empty;
            
            if (root.TryGetProperty("device_type", out var deviceTypeElement))
            {
                device.ConsoleType = deviceTypeElement.GetString()?.ToLowerInvariant() switch
                {
                    "xbox_one" => XboxConsoleType.XboxOne,
                    "xbox_one_s" => XboxConsoleType.XboxOneS,
                    "xbox_one_x" => XboxConsoleType.XboxOneX,
                    "xbox_series_s" => XboxConsoleType.XboxSeriesS,
                    "xbox_series_x" => XboxConsoleType.XboxSeriesX,
                    _ => XboxConsoleType.Unknown
                };
            }
            
            if (root.TryGetProperty("certificate", out var certElement))
                device.Certificate = certElement.GetString() ?? string.Empty;
            
            return device;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to parse Xbox discovery response");
            return null;
        }
    }
}