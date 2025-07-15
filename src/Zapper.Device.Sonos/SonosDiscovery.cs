using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Zapper.Core.Models;

namespace Zapper.Device.Sonos;

public class SonosDiscovery(ILogger<SonosDiscovery> logger, HttpClient httpClient) : ISonosDiscovery, IDisposable
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

            logger.LogInformation("Discovered {Count} Sonos devices", _discoveredDevices.Count);
            return _discoveredDevices.ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during Sonos device discovery");
            return [];
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

            // Send SSDP M-SEARCH request for Sonos devices
            var searchMessage =
                "M-SEARCH * HTTP/1.1\r\n" +
                "HOST: 239.255.255.250:1900\r\n" +
                "MX: 30\r\n" +
                "MAN: \"ssdp:discover\"\r\n" +
                "ST: urn:schemas-upnp-org:device:ZonePlayer:1\r\n\r\n";

            var searchBytes = Encoding.UTF8.GetBytes(searchMessage);
            await _udpClient.SendAsync(searchBytes, searchBytes.Length, multicastEndpoint);

            logger.LogDebug("Sent SSDP discovery request for Sonos devices");

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

                    if (response.Contains("ZonePlayer") || response.Contains("Sonos"))
                    {
                        var device = await ParseSsdpResponseAsync(response, result.RemoteEndPoint, cancellationToken);
                        if (device != null && !_discoveredDevices.Any(d => d.IpAddress == device.IpAddress))
                        {
                            _discoveredDevices.Add(device);
                            DeviceDiscovered?.Invoke(this, device);
                            logger.LogInformation("Discovered Sonos device: {DeviceName} at {IpAddress}", device.Name, device.IpAddress);
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

            // Verify this is actually a Sonos device by checking the device info
            var device = await DiscoverDeviceByIpAsync(ipAddress, cancellationToken);
            if (device != null)
            {
                // Try to get more detailed device information from device description
                try
                {
                    var deviceInfoResponse = await httpClient.GetAsync(locationUrl, cancellationToken);
                    if (deviceInfoResponse.IsSuccessStatusCode)
                    {
                        var deviceInfoXml = await deviceInfoResponse.Content.ReadAsStringAsync(cancellationToken);
                        var xmlDoc = XDocument.Parse(deviceInfoXml);

                        var deviceNode = xmlDoc.Descendants().FirstOrDefault(x => x.Name.LocalName == "device");
                        if (deviceNode != null)
                        {
                            var friendlyName = deviceNode.Descendants().FirstOrDefault(x => x.Name.LocalName == "friendlyName")?.Value;
                            var modelName = deviceNode.Descendants().FirstOrDefault(x => x.Name.LocalName == "modelName")?.Value;
                            var manufacturer = deviceNode.Descendants().FirstOrDefault(x => x.Name.LocalName == "manufacturer")?.Value;

                            if (!string.IsNullOrEmpty(friendlyName))
                                device.Name = friendlyName;
                            if (!string.IsNullOrEmpty(modelName))
                                device.Model = modelName;
                            if (!string.IsNullOrEmpty(manufacturer))
                                device.Brand = manufacturer;
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogDebug(ex, "Could not get detailed device info for Sonos at {IpAddress}", ipAddress);
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

    private async Task<Zapper.Core.Models.Device?> DiscoverDeviceByIpAsync(string ipAddress, CancellationToken cancellationToken)
    {
        try
        {
            var url = $"http://{ipAddress}:1400/xml/device_description.xml";
            var response = await httpClient.GetAsync(url, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var device = new Zapper.Core.Models.Device
                {
                    Name = $"Sonos Speaker ({ipAddress})",
                    Brand = "Sonos",
                    Model = "Unknown",
                    Type = DeviceType.Sonos,
                    ConnectionType = ConnectionType.Network,
                    IpAddress = ipAddress,
                    Port = 1400,
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
            logger.LogDebug(ex, "Failed to discover Sonos device at {IpAddress}", ipAddress);
            return null;
        }
    }

    public void Dispose()
    {
        _udpClient?.Dispose();
        GC.SuppressFinalize(this);
    }
}