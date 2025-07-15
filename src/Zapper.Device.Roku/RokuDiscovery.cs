using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Zapper.Core.Models;
using Zapper.Device.Network;

namespace Zapper.Device.Roku;

public class RokuDiscovery(ILogger<RokuDiscovery> logger, INetworkDeviceController networkController) : IRokuDiscovery, IDisposable
{
    private UdpClient? _udpClient;
    private readonly List<Zapper.Core.Models.Device> _discoveredDevices = [];

    public event EventHandler<Zapper.Core.Models.Device>? DeviceDiscovered;

    public async Task<IEnumerable<Zapper.Core.Models.Device>> DiscoverDevicesAsync(TimeSpan timeout = default, CancellationToken cancellationToken = default)
    {
        if (timeout == default)
            timeout = TimeSpan.FromSeconds(10);

        _discoveredDevices.Clear();

        try
        {
            await PerformSsdpDiscoveryAsync(timeout, cancellationToken);

            logger.LogInformation("Discovered {Count} Roku devices", _discoveredDevices.Count);
            return _discoveredDevices.ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during Roku device discovery");
            return [];
        }
    }

    public async Task<Zapper.Core.Models.Device?> DiscoverDeviceByIpAsync(string ipAddress, CancellationToken cancellationToken = default)
    {
        try
        {
            var baseUrl = $"http://{ipAddress}:8060";
            var success = await networkController.SendHttpCommandAsync(baseUrl, "/", "GET", null, null, cancellationToken);

            if (success)
            {
                var device = new Zapper.Core.Models.Device
                {
                    Name = $"Roku Device ({ipAddress})",
                    Brand = "Roku",
                    Model = "Unknown",
                    Type = DeviceType.StreamingDevice,
                    ConnectionType = ConnectionType.NetworkHttp,
                    IpAddress = ipAddress,
                    Port = 8060,
                    IsOnline = true,
                    CreatedAt = DateTime.UtcNow,
                    LastSeen = DateTime.UtcNow
                };

                return device;
            }

            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to discover Roku device at {IpAddress}", ipAddress);
            return null;
        }
    }

    private async Task PerformSsdpDiscoveryAsync(TimeSpan timeout, CancellationToken cancellationToken)
    {
        try
        {
            _udpClient?.Dispose();
            _udpClient = new UdpClient();
            _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            var multicastEndpoint = new IPEndPoint(IPAddress.Parse("239.255.255.250"), 1900);

            // Send SSDP M-SEARCH request for Roku devices
            var searchMessage =
                "M-SEARCH * HTTP/1.1\r\n" +
                "HOST: 239.255.255.250:1900\r\n" +
                "MX: 30\r\n" +
                "MAN: \"ssdp:discover\"\r\n" +
                "ST: roku:ecp\r\n\r\n";

            var searchBytes = Encoding.UTF8.GetBytes(searchMessage);
            await _udpClient.SendAsync(searchBytes, searchBytes.Length, multicastEndpoint);

            logger.LogDebug("Sent SSDP discovery request for Roku devices");

            // Listen for responses
            var endTime = DateTime.UtcNow.Add(timeout);
            while (DateTime.UtcNow < endTime && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    timeoutCts.CancelAfter(TimeSpan.FromSeconds(1));

                    var result = await _udpClient.ReceiveAsync().WaitAsync(timeoutCts.Token);
                    var response = Encoding.UTF8.GetString(result.Buffer);

                    if (response.Contains("roku:ecp") || response.Contains("Roku"))
                    {
                        var device = await ParseSsdpResponseAsync(response, result.RemoteEndPoint, cancellationToken);
                        if (device != null && !_discoveredDevices.Any(d => d.IpAddress == device.IpAddress))
                        {
                            _discoveredDevices.Add(device);
                            DeviceDiscovered?.Invoke(this, device);
                            logger.LogInformation("Discovered Roku device: {DeviceName} at {IpAddress}", device.Name, device.IpAddress);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // Timeout is expected, continue listening
                }
                catch (Exception ex)
                {
                    logger.LogDebug(ex, "Error receiving SSDP response");
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during SSDP discovery");
        }
    }

    private async Task<Zapper.Core.Models.Device?> ParseSsdpResponseAsync(string response, IPEndPoint remoteEndpoint, CancellationToken cancellationToken)
    {
        try
        {
            // Extract the LOCATION header from SSDP response
            var locationMatch = Regex.Match(response, @"LOCATION:\s*(.+)", RegexOptions.IgnoreCase);
            if (!locationMatch.Success)
            {
                return null;
            }

            var locationUrl = locationMatch.Groups[1].Value.Trim();

            // Parse the IP address from the location URL
            var uri = new Uri(locationUrl);
            var ipAddress = uri.Host;

            // Verify this is actually a Roku device by checking the device info
            var device = await DiscoverDeviceByIpAsync(ipAddress, cancellationToken);
            if (device != null)
            {
                // Try to get more detailed device information
                try
                {
                    var baseUrl = $"http://{ipAddress}:8060";
                    var success = await networkController.SendHttpCommandAsync(baseUrl, "/", "GET", null, null, cancellationToken);

                    if (success)
                    {
                        // In a real implementation, we'd parse the XML response to get device details
                        device.Name = $"Roku Device ({ipAddress})";
                        device.Brand = "Roku";
                        device.Model = "Unknown";
                    }
                }
                catch (Exception ex)
                {
                    logger.LogDebug(ex, "Could not get detailed device info for Roku at {IpAddress}", ipAddress);
                }
            }

            return device;
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Error parsing SSDP response from {RemoteEndpoint}", remoteEndpoint);
            return null;
        }
    }

    public void Dispose()
    {
        _udpClient?.Dispose();
        GC.SuppressFinalize(this);
    }
}