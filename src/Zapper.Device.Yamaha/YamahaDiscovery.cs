using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Zapper.Core.Models;

namespace Zapper.Device.Yamaha;

public class YamahaDiscovery(ILogger<YamahaDiscovery> logger, HttpClient httpClient) : IYamahaDiscovery, IDisposable
{
    private readonly List<Zapper.Core.Models.Device> _discoveredDevices = [];

    public event EventHandler<Zapper.Core.Models.Device>? DeviceDiscovered;

    public async Task<IEnumerable<Zapper.Core.Models.Device>> DiscoverDevicesAsync(TimeSpan timeout = default, CancellationToken cancellationToken = default)
    {
        if (timeout == default)
            timeout = TimeSpan.FromSeconds(10);

        _discoveredDevices.Clear();

        try
        {
            await PerformNetworkScanAsync(timeout, cancellationToken);

            logger.LogInformation("Discovered {Count} Yamaha devices", _discoveredDevices.Count);
            return _discoveredDevices.ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during Yamaha device discovery");
            return [];
        }
    }

    private async Task PerformNetworkScanAsync(TimeSpan timeout, CancellationToken cancellationToken)
    {
        try
        {
            var localIPs = GetLocalNetworkAddresses();
            var tasks = new List<Task>();

            foreach (var localIp in localIPs)
            {
                var networkBase = GetNetworkBase(localIp);
                for (int i = 1; i < 255; i++)
                {
                    var ipToCheck = $"{networkBase}.{i}";
                    tasks.Add(CheckYamahaDeviceAsync(ipToCheck, cancellationToken));
                }
            }

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(timeout);

            try
            {
                await Task.WhenAll(tasks).WaitAsync(timeoutCts.Token);
            }
            catch (OperationCanceledException)
            {
                logger.LogDebug("Yamaha discovery scan timed out");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during Yamaha network scan");
        }
    }

    private async Task CheckYamahaDeviceAsync(string ipAddress, CancellationToken cancellationToken)
    {
        try
        {
            using var ping = new Ping();
            var reply = await ping.SendPingAsync(ipAddress, 1000);

            if (reply.Status == IPStatus.Success)
            {
                var device = await DiscoverDeviceByIpAsync(ipAddress, cancellationToken);
                if (device != null)
                {
                    lock (_discoveredDevices)
                    {
                        if (!_discoveredDevices.Any(d => d.IpAddress == device.IpAddress))
                        {
                            _discoveredDevices.Add(device);
                            DeviceDiscovered?.Invoke(this, device);
                            logger.LogInformation("Discovered Yamaha device: {DeviceName} at {IpAddress}", device.Name, device.IpAddress);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Could not check Yamaha device at {IpAddress}", ipAddress);
        }
    }

    private async Task<Zapper.Core.Models.Device?> DiscoverDeviceByIpAsync(string ipAddress, CancellationToken cancellationToken)
    {
        try
        {
            // Try MusicCast API first
            var musicCastUrl = $"http://{ipAddress}/YamahaExtendedControl/v1/system/getFeatures";

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(3));

            var response = await httpClient.GetAsync(musicCastUrl, timeoutCts.Token);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(timeoutCts.Token);

                // Try to parse response to get device info
                try
                {
                    var features = JsonSerializer.Deserialize<YamahaFeaturesResponse>(content, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                    });

                    var device = new Zapper.Core.Models.Device
                    {
                        Name = $"Yamaha Receiver ({ipAddress})",
                        Brand = "Yamaha",
                        Model = features?.System?.ModelName ?? "MusicCast Device",
                        Type = DeviceType.YamahaReceiver,
                        ConnectionType = ConnectionType.Network,
                        IpAddress = ipAddress,
                        Port = 80,
                        IsOnline = true,
                        CreatedAt = DateTime.UtcNow,
                        LastSeen = DateTime.UtcNow
                    };

                    return device;
                }
                catch (JsonException)
                {
                    // Response wasn't valid JSON, but device responded - treat as basic Yamaha device
                    var device = new Zapper.Core.Models.Device
                    {
                        Name = $"Yamaha Receiver ({ipAddress})",
                        Brand = "Yamaha",
                        Model = "Unknown",
                        Type = DeviceType.YamahaReceiver,
                        ConnectionType = ConnectionType.Network,
                        IpAddress = ipAddress,
                        Port = 80,
                        IsOnline = true,
                        CreatedAt = DateTime.UtcNow,
                        LastSeen = DateTime.UtcNow
                    };

                    return device;
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Failed to discover Yamaha device at {IpAddress}", ipAddress);
            return null;
        }
    }

    private static List<IPAddress> GetLocalNetworkAddresses()
    {
        var addresses = new List<IPAddress>();

        try
        {
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus == OperationalStatus.Up &&
                    ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {
                    foreach (var addr in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (addr.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            addresses.Add(addr.Address);
                        }
                    }
                }
            }
        }
        catch (Exception)
        {
            // Fallback to basic approach
            addresses.Add(IPAddress.Parse("192.168.1.1"));
        }

        return addresses;
    }

    private static string GetNetworkBase(IPAddress ipAddress)
    {
        var ipBytes = ipAddress.GetAddressBytes();
        return $"{ipBytes[0]}.{ipBytes[1]}.{ipBytes[2]}";
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    private class YamahaFeaturesResponse
    {
        public YamahaSystemInfo? System { get; set; }
    }

    private class YamahaSystemInfo
    {
        public string? ModelName { get; set; }
        public string? Version { get; set; }
    }
}