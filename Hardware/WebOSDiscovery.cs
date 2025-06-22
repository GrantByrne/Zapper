using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using ZapperHub.Models;

namespace ZapperHub.Hardware;

public class WebOSDiscovery : IWebOSDiscovery, IDisposable
{
    private readonly ILogger<WebOSDiscovery> _logger;
    private readonly IWebOSClient _webOSClient;
    private UdpClient? _udpClient;
    private readonly List<Device> _discoveredDevices = new();

    public event EventHandler<Device>? DeviceDiscovered;

    public WebOSDiscovery(ILogger<WebOSDiscovery> logger, IWebOSClient webOSClient)
    {
        _logger = logger;
        _webOSClient = webOSClient;
    }

    public async Task<IEnumerable<Device>> DiscoverDevicesAsync(TimeSpan timeout = default, CancellationToken cancellationToken = default)
    {
        if (timeout == default)
            timeout = TimeSpan.FromSeconds(10);

        _discoveredDevices.Clear();

        try
        {
            // Start SSDP discovery
            var ssdpTask = PerformSsdpDiscoveryAsync(timeout, cancellationToken);
            
            // Start mDNS discovery
            var mdnsTask = PerformMdnsDiscoveryAsync(timeout, cancellationToken);

            // Wait for both discovery methods to complete
            await Task.WhenAll(ssdpTask, mdnsTask);

            _logger.LogInformation("Discovered {Count} WebOS devices", _discoveredDevices.Count);
            return _discoveredDevices.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during WebOS device discovery");
            return Enumerable.Empty<Device>();
        }
    }

    public async Task<Device?> DiscoverDeviceByIpAsync(string ipAddress, CancellationToken cancellationToken = default)
    {
        try
        {
            // Try to connect to the WebOS TV directly
            var connected = await _webOSClient.ConnectAsync(ipAddress, false, cancellationToken);
            if (!connected)
            {
                // Try secure connection
                connected = await _webOSClient.ConnectAsync(ipAddress, true, cancellationToken);
            }

            if (connected)
            {
                var device = new Device
                {
                    Name = $"WebOS TV ({ipAddress})",
                    Brand = "LG",
                    Model = "Unknown",
                    Type = DeviceType.SmartTV,
                    ConnectionType = ConnectionType.WebOS,
                    NetworkAddress = ipAddress,
                    UseSecureConnection = false,
                    IsOnline = true,
                    CreatedAt = DateTime.UtcNow,
                    LastSeen = DateTime.UtcNow
                };

                await _webOSClient.DisconnectAsync(cancellationToken);
                return device;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to discover WebOS device at {IpAddress}", ipAddress);
            return null;
        }
    }

    public async Task<bool> PairWithDeviceAsync(Device device, CancellationToken cancellationToken = default)
    {
        if (device.ConnectionType != ConnectionType.WebOS || string.IsNullOrEmpty(device.NetworkAddress))
        {
            _logger.LogWarning("Device {DeviceName} is not a valid WebOS device for pairing", device.Name);
            return false;
        }

        try
        {
            // Connect to the device
            var connected = await _webOSClient.ConnectAsync(device.NetworkAddress, device.UseSecureConnection, cancellationToken);
            if (!connected)
            {
                _logger.LogError("Failed to connect to WebOS device {DeviceName} for pairing", device.Name);
                return false;
            }

            // Attempt authentication (this will trigger pairing prompt on TV)
            var authenticated = await _webOSClient.AuthenticateAsync(device.AuthenticationToken, cancellationToken);
            if (authenticated)
            {
                // Store the client key for future connections
                device.AuthenticationToken = _webOSClient.ClientKey;
                device.IsOnline = true;
                device.LastSeen = DateTime.UtcNow;

                _logger.LogInformation("Successfully paired with WebOS device {DeviceName}", device.Name);
                return true;
            }

            _logger.LogWarning("Failed to authenticate with WebOS device {DeviceName}", device.Name);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during pairing with WebOS device {DeviceName}", device.Name);
            return false;
        }
        finally
        {
            await _webOSClient.DisconnectAsync(cancellationToken);
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
            
            // Send SSDP M-SEARCH request for WebOS devices
            var searchMessage = 
                "M-SEARCH * HTTP/1.1\r\n" +
                "HOST: 239.255.255.250:1900\r\n" +
                "MX: 30\r\n" +
                "MAN: \"ssdp:discover\"\r\n" +
                "ST: urn:lge-com:service:webos-second-screen:1\r\n\r\n";

            var searchBytes = Encoding.UTF8.GetBytes(searchMessage);
            await _udpClient.SendAsync(searchBytes, searchBytes.Length, multicastEndpoint);

            _logger.LogDebug("Sent SSDP discovery request for WebOS devices");

            // Listen for responses
            var endTime = DateTime.UtcNow.Add(timeout);
            while (DateTime.UtcNow < endTime && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    timeoutCts.CancelAfter(TimeSpan.FromSeconds(1));

                    var result = await _udpClient.ReceiveAsync().WaitAsync(timeoutCts.Token);
                    await ProcessSsdpResponseAsync(result.Buffer, result.RemoteEndPoint);
                }
                catch (OperationCanceledException)
                {
                    // Timeout or cancellation - continue loop
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Error receiving SSDP response");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during SSDP discovery");
        }
    }

    private async Task PerformMdnsDiscoveryAsync(TimeSpan timeout, CancellationToken cancellationToken)
    {
        try
        {
            // Simple mDNS-style discovery by checking common hostnames
            var commonHostnames = new[] { "lgsmarttv.local", "webostv.local" };
            
            foreach (var hostname in commonHostnames)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                try
                {
                    var hostEntry = await Dns.GetHostEntryAsync(hostname, cancellationToken);
                    foreach (var address in hostEntry.AddressList)
                    {
                        if (address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            var device = await DiscoverDeviceByIpAsync(address.ToString(), cancellationToken);
                            if (device != null)
                            {
                                device.Name = $"WebOS TV ({hostname})";
                                AddDiscoveredDevice(device);
                            }
                        }
                    }
                }
                catch (SocketException)
                {
                    // Hostname not found - this is expected
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Error resolving hostname {Hostname}", hostname);
                }
            }

            // Also try network scanning on local subnet
            await ScanLocalSubnetAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during mDNS discovery");
        }
    }

    private async Task ScanLocalSubnetAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Get local IP addresses
            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(ni => ni.OperationalStatus == OperationalStatus.Up && 
                           ni.NetworkInterfaceType != NetworkInterfaceType.Loopback);

            foreach (var networkInterface in networkInterfaces)
            {
                var ipProperties = networkInterface.GetIPProperties();
                foreach (var unicastAddress in ipProperties.UnicastAddresses)
                {
                    if (unicastAddress.Address.AddressFamily == AddressFamily.InterNetwork &&
                        !IPAddress.IsLoopback(unicastAddress.Address))
                    {
                        await ScanSubnetAsync(unicastAddress.Address, unicastAddress.IPv4Mask, cancellationToken);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Error during subnet scanning");
        }
    }

    private async Task ScanSubnetAsync(IPAddress localIp, IPAddress subnetMask, CancellationToken cancellationToken)
    {
        try
        {
            var network = GetNetworkAddress(localIp, subnetMask);
            var broadcastAddress = GetBroadcastAddress(localIp, subnetMask);
            
            // Only scan a subset of addresses to avoid being too aggressive
            var tasks = new List<Task>();
            var addressesToScan = GenerateAddressesToScan(network, broadcastAddress, 20); // Limit to 20 addresses

            foreach (var address in addressesToScan)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                tasks.Add(CheckAddressForWebOSAsync(address, cancellationToken));
                
                // Limit concurrent scans
                if (tasks.Count >= 5)
                {
                    await Task.WhenAny(tasks);
                    tasks.RemoveAll(t => t.IsCompleted);
                }
            }

            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Error scanning subnet");
        }
    }

    private async Task CheckAddressForWebOSAsync(IPAddress address, CancellationToken cancellationToken)
    {
        try
        {
            using var ping = new Ping();
            var reply = await ping.SendPingAsync(address, 1000);
            
            if (reply.Status == IPStatus.Success)
            {
                var device = await DiscoverDeviceByIpAsync(address.ToString(), cancellationToken);
                if (device != null)
                {
                    AddDiscoveredDevice(device);
                }
            }
        }
        catch (Exception)
        {
            // Ignore errors during ping/discovery
        }
    }

    private async Task ProcessSsdpResponseAsync(byte[] responseBytes, IPEndPoint remoteEndPoint)
    {
        try
        {
            var response = Encoding.UTF8.GetString(responseBytes);
            
            // Check if this is a WebOS device response
            if (response.Contains("urn:lge-com:service:webos-second-screen") ||
                response.Contains("LG Electronics"))
            {
                // Extract device information from SSDP response
                var locationMatch = Regex.Match(response, @"LOCATION:\s*(.+)", RegexOptions.IgnoreCase);
                var serverMatch = Regex.Match(response, @"SERVER:\s*(.+)", RegexOptions.IgnoreCase);

                var device = new Device
                {
                    Name = $"WebOS TV ({remoteEndPoint.Address})",
                    Brand = "LG",
                    Model = serverMatch.Success ? ExtractModelFromServer(serverMatch.Groups[1].Value) : "Unknown",
                    Type = DeviceType.SmartTV,
                    ConnectionType = ConnectionType.WebOS,
                    NetworkAddress = remoteEndPoint.Address.ToString(),
                    UseSecureConnection = false,
                    IsOnline = true,
                    CreatedAt = DateTime.UtcNow,
                    LastSeen = DateTime.UtcNow
                };

                AddDiscoveredDevice(device);
                _logger.LogDebug("Discovered WebOS device via SSDP: {DeviceName}", device.Name);
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Error processing SSDP response");
        }
    }

    private void AddDiscoveredDevice(Device device)
    {
        // Avoid duplicates
        if (!_discoveredDevices.Any(d => d.NetworkAddress == device.NetworkAddress))
        {
            _discoveredDevices.Add(device);
            DeviceDiscovered?.Invoke(this, device);
        }
    }

    private static string ExtractModelFromServer(string serverHeader)
    {
        // Try to extract model from server header like "Linux/3.14.0 UPnP/1.0 LG Smart TV/1.0"
        var modelMatch = Regex.Match(serverHeader, @"LG\s+(.+?)/", RegexOptions.IgnoreCase);
        return modelMatch.Success ? modelMatch.Groups[1].Value.Trim() : "Smart TV";
    }

    private static IPAddress GetNetworkAddress(IPAddress ip, IPAddress mask)
    {
        var ipBytes = ip.GetAddressBytes();
        var maskBytes = mask.GetAddressBytes();
        var networkBytes = new byte[4];

        for (int i = 0; i < 4; i++)
        {
            networkBytes[i] = (byte)(ipBytes[i] & maskBytes[i]);
        }

        return new IPAddress(networkBytes);
    }

    private static IPAddress GetBroadcastAddress(IPAddress ip, IPAddress mask)
    {
        var ipBytes = ip.GetAddressBytes();
        var maskBytes = mask.GetAddressBytes();
        var broadcastBytes = new byte[4];

        for (int i = 0; i < 4; i++)
        {
            broadcastBytes[i] = (byte)(ipBytes[i] | ~maskBytes[i]);
        }

        return new IPAddress(broadcastBytes);
    }

    private static IEnumerable<IPAddress> GenerateAddressesToScan(IPAddress network, IPAddress broadcast, int maxAddresses)
    {
        var networkBytes = network.GetAddressBytes();
        var broadcastBytes = broadcast.GetAddressBytes();
        
        var count = 0;
        for (int i = networkBytes[3] + 1; i < broadcastBytes[3] && count < maxAddresses; i++)
        {
            var addressBytes = new byte[] { networkBytes[0], networkBytes[1], networkBytes[2], (byte)i };
            yield return new IPAddress(addressBytes);
            count++;
        }
    }

    public void Dispose()
    {
        _udpClient?.Dispose();
    }
}