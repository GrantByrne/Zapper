using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Zapper.Core.Models;
using Microsoft.Extensions.Logging;

namespace Zapper.Device.Tizen;

public class TizenDiscovery(ILogger<TizenDiscovery> logger, ITizenClient tizenClient) : ITizenDiscovery, IDisposable
{
    private UdpClient? _udpClient;
    private readonly List<Zapper.Core.Models.Device> _discoveredDevices = [];
    private const string SsdpSearchMessage = "M-SEARCH * HTTP/1.1\r\nHOST: 239.255.255.250:1900\r\nMAN: \"ssdp:discover\"\r\nMX: 3\r\nST: urn:samsung:device:RemoteControlReceiver:1\r\n\r\n";
    private const int SsdpPort = 1900;
    private const string SsdpMulticastAddress = "239.255.255.250";

    public event EventHandler<Zapper.Core.Models.Device>? DeviceDiscovered;

    public async Task<IEnumerable<Zapper.Core.Models.Device>> DiscoverDevices(TimeSpan timeout = default, CancellationToken cancellationToken = default)
    {
        if (timeout == default)
            timeout = TimeSpan.FromSeconds(10);

        _discoveredDevices.Clear();

        try
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(timeout);

            await PerformSsdpDiscoveryAsync(timeoutCts.Token);

            logger.LogInformation("Discovered {Count} Samsung Tizen devices", _discoveredDevices.Count);
            return _discoveredDevices.ToList();
        }
        catch (OperationCanceledException)
        {
            logger.LogDebug("Samsung Tizen device discovery was cancelled or timed out");
            return _discoveredDevices.ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during Samsung Tizen device discovery");
            return [];
        }
    }

    public async Task<Zapper.Core.Models.Device?> DiscoverDeviceByIp(string ipAddress, CancellationToken cancellationToken = default)
    {
        try
        {
            var connected = await tizenClient.ConnectAsync(ipAddress, null, cancellationToken);

            if (connected)
            {
                var device = new Zapper.Core.Models.Device
                {
                    Name = $"Samsung TV ({ipAddress})",
                    Brand = "Samsung",
                    Model = "Unknown",
                    Type = DeviceType.TizenTv,
                    ConnectionType = ConnectionType.Tizen,
                    NetworkAddress = ipAddress,
                    UseSecureConnection = true,
                    IsOnline = true,
                    CreatedAt = DateTime.UtcNow,
                    LastSeen = DateTime.UtcNow
                };

                await tizenClient.DisconnectAsync(cancellationToken);

                return device;
            }

            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to discover Samsung TV at {IpAddress}", ipAddress);
            return null;
        }
    }

    public async Task<bool> PairWithDevice(Zapper.Core.Models.Device device, string? pinCode = null, CancellationToken cancellationToken = default)
    {
        if (device.ConnectionType != ConnectionType.Tizen)
        {
            logger.LogWarning("Device {DeviceName} is not a Tizen device", device.Name);
            return false;
        }

        try
        {
            var connected = await tizenClient.ConnectAsync(device.NetworkAddress!, device.AuthenticationToken, cancellationToken);
            if (!connected)
            {
                logger.LogError("Failed to connect to Tizen device {DeviceName}", device.Name);
                return false;
            }

            var token = await tizenClient.AuthenticateAsync("Zapper", cancellationToken);
            if (string.IsNullOrEmpty(token))
            {
                logger.LogError("Failed to authenticate with Tizen device {DeviceName}", device.Name);
                return false;
            }

            device.AuthenticationToken = token;
            device.LastSeen = DateTime.UtcNow;

            await tizenClient.DisconnectAsync(cancellationToken);

            logger.LogInformation("Successfully paired with Tizen device {DeviceName}", device.Name);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to pair with Tizen device {DeviceName}", device.Name);
            return false;
        }
    }

    private async Task PerformSsdpDiscoveryAsync(CancellationToken cancellationToken)
    {
        try
        {
            _udpClient = new UdpClient();
            _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _udpClient.EnableBroadcast = true;

            var searchBytes = Encoding.UTF8.GetBytes(SsdpSearchMessage);
            var endPoint = new IPEndPoint(IPAddress.Parse(SsdpMulticastAddress), SsdpPort);

            await _udpClient.SendAsync(searchBytes, searchBytes.Length, endPoint);

            var receiveTask = Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var result = await _udpClient.ReceiveAsync();
                        var response = Encoding.UTF8.GetString(result.Buffer);

                        if (response.Contains("samsung", StringComparison.OrdinalIgnoreCase))
                        {
                            await ProcessSsdpResponseAsync(response, result.RemoteEndPoint.Address.ToString());
                        }
                    }
                    catch (ObjectDisposedException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        logger.LogDebug(ex, "Error receiving SSDP response");
                    }
                }
            }, cancellationToken);

            try
            {
                await receiveTask;
            }
            catch (OperationCanceledException)
            {
            }
        }
        finally
        {
            _udpClient?.Close();
            _udpClient?.Dispose();
            _udpClient = null;
        }
    }

    private async Task ProcessSsdpResponseAsync(string response, string remoteAddress)
    {
        try
        {
            var locationMatch = Regex.Match(response, @"LOCATION:\s*(.+)", RegexOptions.IgnoreCase);
            if (!locationMatch.Success)
                return;

            var location = locationMatch.Groups[1].Value.Trim();
            var deviceInfo = await FetchDeviceInfoAsync(location);

            if (deviceInfo != null)
            {
                var device = new Zapper.Core.Models.Device
                {
                    Name = deviceInfo.FriendlyName ?? $"Samsung TV ({remoteAddress})",
                    Brand = "Samsung",
                    Model = deviceInfo.Model ?? "Unknown",
                    Type = DeviceType.TizenTv,
                    ConnectionType = ConnectionType.Tizen,
                    NetworkAddress = remoteAddress,
                    UseSecureConnection = true,
                    IsOnline = true,
                    CreatedAt = DateTime.UtcNow,
                    LastSeen = DateTime.UtcNow
                };

                if (!_discoveredDevices.Any(d => d.NetworkAddress == device.NetworkAddress))
                {
                    _discoveredDevices.Add(device);
                    DeviceDiscovered?.Invoke(this, device);
                    logger.LogInformation("Discovered Samsung TV: {DeviceName} at {IpAddress}", device.Name, device.NetworkAddress);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process SSDP response");
        }
    }

    private async Task<DeviceInfo?> FetchDeviceInfoAsync(string location)
    {
        try
        {
            using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            var xml = await httpClient.GetStringAsync(location);

            var doc = XDocument.Parse(xml);
            var ns = doc.Root?.GetDefaultNamespace() ?? XNamespace.None;

            var deviceElement = doc.Descendants(ns + "device").FirstOrDefault();
            if (deviceElement == null)
                return null;

            return new DeviceInfo
            {
                FriendlyName = deviceElement.Element(ns + "friendlyName")?.Value,
                Model = deviceElement.Element(ns + "modelName")?.Value,
                SerialNumber = deviceElement.Element(ns + "serialNumber")?.Value
            };
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Failed to fetch device info from {Location}", location);
            return null;
        }
    }

    public void Dispose()
    {
        _udpClient?.Close();
        _udpClient?.Dispose();
    }

    private class DeviceInfo
    {
        public string? FriendlyName { get; set; }
        public string? Model { get; set; }
        public string? SerialNumber { get; set; }
    }
}