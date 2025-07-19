using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;

namespace Zapper.Device.Denon;

public class DenonDiscovery(IHttpClientFactory httpClientFactory, ILogger<DenonDiscovery> logger) : IDenonDiscovery
{
    private const int DenonPort = 80;
    private const int DiscoveryTimeoutMs = 5000;
    private const int SsdpPort = 1900;
    private const string SsdpMulticastAddress = "239.255.255.250";

    public async Task<IEnumerable<DenonDevice>> DiscoverDevicesAsync(CancellationToken cancellationToken = default)
    {
        var devices = new List<DenonDevice>();

        try
        {
            var ssdpDevices = await DiscoverViaSsdpAsync(cancellationToken);
            devices.AddRange(ssdpDevices);

            if (devices.Count == 0)
            {
                logger.LogInformation("No devices found via SSDP, attempting network scan");
                var networkDevices = await ScanNetworkAsync(cancellationToken);
                devices.AddRange(networkDevices);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during device discovery");
        }

        return devices.Distinct();
    }

    private async Task<List<DenonDevice>> DiscoverViaSsdpAsync(CancellationToken cancellationToken)
    {
        var devices = new List<DenonDevice>();

        try
        {
            using var udpClient = new UdpClient();
            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, 0));

            var searchMessage = Encoding.UTF8.GetBytes(
                "M-SEARCH * HTTP/1.1\r\n" +
                $"HOST: {SsdpMulticastAddress}:{SsdpPort}\r\n" +
                "MAN: \"ssdp:discover\"\r\n" +
                "MX: 3\r\n" +
                "ST: urn:schemas-denon-com:device:ACT-Denon:1\r\n" +
                "\r\n");

            var endpoint = new IPEndPoint(IPAddress.Parse(SsdpMulticastAddress), SsdpPort);
            await udpClient.SendAsync(searchMessage, searchMessage.Length, endpoint);

            udpClient.Client.ReceiveTimeout = DiscoveryTimeoutMs;

            var startTime = DateTime.UtcNow;
            while (DateTime.UtcNow - startTime < TimeSpan.FromMilliseconds(DiscoveryTimeoutMs))
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                try
                {
                    var result = await udpClient.ReceiveAsync();
                    var response = Encoding.UTF8.GetString(result.Buffer);

                    if (response.Contains("denon", StringComparison.OrdinalIgnoreCase) ||
                        response.Contains("marantz", StringComparison.OrdinalIgnoreCase))
                    {
                        var locationLine = response.Split('\n')
                            .FirstOrDefault(line => line.StartsWith("LOCATION:", StringComparison.OrdinalIgnoreCase));

                        if (!string.IsNullOrEmpty(locationLine))
                        {
                            var locationUrl = locationLine.Split(':', 2)[1].Trim();
                            var device = await GetDeviceDetailsFromLocationAsync(locationUrl, cancellationToken);
                            if (device != null)
                            {
                                devices.Add(device);
                            }
                        }
                    }
                }
                catch (SocketException)
                {
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during SSDP discovery");
        }

        return devices;
    }

    private async Task<List<DenonDevice>> ScanNetworkAsync(CancellationToken cancellationToken)
    {
        var devices = new List<DenonDevice>();
        var localIps = GetLocalIPAddresses();

        var scanTasks = new List<Task<DenonDevice?>>();

        foreach (var localIp in localIps)
        {
            var subnet = GetSubnet(localIp);
            if (string.IsNullOrEmpty(subnet))
                continue;

            for (int i = 1; i <= 254; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                var ipToCheck = $"{subnet}.{i}";
                scanTasks.Add(CheckDenonDeviceAsync(ipToCheck, cancellationToken));

                if (scanTasks.Count >= 20)
                {
                    var completedDevices = await Task.WhenAll(scanTasks);
                    devices.AddRange(completedDevices.Where(d => d != null)!);
                    scanTasks.Clear();
                }
            }
        }

        if (scanTasks.Any())
        {
            var completedDevices = await Task.WhenAll(scanTasks);
            devices.AddRange(completedDevices.Where(d => d != null)!);
        }

        return devices;
    }

    private async Task<DenonDevice?> CheckDenonDeviceAsync(string ipAddress, CancellationToken cancellationToken)
    {
        try
        {
            using var httpClient = httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri($"http://{ipAddress}:{DenonPort}");
            httpClient.Timeout = TimeSpan.FromMilliseconds(2000);

            var response = await httpClient.GetAsync("/goform/Deviceinfo.xml", cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                return ParseDeviceInfo(content, ipAddress);
            }
        }
        catch
        {
        }

        return null;
    }

    private async Task<DenonDevice?> GetDeviceDetailsFromLocationAsync(string locationUrl, CancellationToken cancellationToken)
    {
        try
        {
            var uri = new Uri(locationUrl);
            var ipAddress = uri.Host;

            using var httpClient = httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromMilliseconds(3000);

            var deviceInfoUrl = $"http://{ipAddress}:{DenonPort}/goform/Deviceinfo.xml";
            var response = await httpClient.GetAsync(deviceInfoUrl, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                return ParseDeviceInfo(content, ipAddress);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting device details from {Location}", locationUrl);
        }

        return null;
    }

    private DenonDevice? ParseDeviceInfo(string xmlContent, string ipAddress)
    {
        try
        {
            var doc = XDocument.Parse(xmlContent);
            var root = doc.Root;

            if (root == null)
                return null;

            var model = root.Element("ModelName")?.Value ?? "Unknown Denon Model";
            var name = root.Element("FriendlyName")?.Value ?? model;
            var serialNumber = root.Element("SerialNumber")?.Value ?? "Unknown";

            if (model.Contains("Denon", StringComparison.OrdinalIgnoreCase) ||
                model.Contains("Marantz", StringComparison.OrdinalIgnoreCase) ||
                name.Contains("Denon", StringComparison.OrdinalIgnoreCase) ||
                name.Contains("Marantz", StringComparison.OrdinalIgnoreCase))
            {
                return new DenonDevice(ipAddress, model, name, serialNumber);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error parsing device info XML");
        }

        return null;
    }

    private List<string> GetLocalIPAddresses()
    {
        var addresses = new List<string>();

        try
        {
            foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (networkInterface.OperationalStatus != OperationalStatus.Up ||
                    networkInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                    continue;

                foreach (var address in networkInterface.GetIPProperties().UnicastAddresses)
                {
                    if (address.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        addresses.Add(address.Address.ToString());
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting local IP addresses");
        }

        return addresses;
    }

    private string GetSubnet(string ipAddress)
    {
        try
        {
            var parts = ipAddress.Split('.');
            if (parts.Length == 4)
            {
                return $"{parts[0]}.{parts[1]}.{parts[2]}";
            }
        }
        catch
        {
        }

        return string.Empty;
    }
}