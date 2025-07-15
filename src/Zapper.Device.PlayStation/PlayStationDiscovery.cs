using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;
using Zapper.Core.Models;

namespace Zapper.Device.PlayStation;

public class PlayStationDiscovery(ILogger<PlayStationDiscovery> logger) : IPlayStationDiscovery, IDisposable
{
    private readonly List<Zapper.Core.Models.Device> _discoveredDevices = [];
    private const int RemotePlayPort = 9295;

    public event EventHandler<Zapper.Core.Models.Device>? DeviceDiscovered;

    public async Task<IEnumerable<Zapper.Core.Models.Device>> DiscoverDevicesAsync(TimeSpan timeout = default, CancellationToken cancellationToken = default)
    {
        if (timeout == default)
            timeout = TimeSpan.FromSeconds(10);

        _discoveredDevices.Clear();

        try
        {
            await PerformNetworkScanAsync(timeout, cancellationToken);

            logger.LogInformation("Discovered {Count} PlayStation devices", _discoveredDevices.Count);
            return _discoveredDevices.ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during PlayStation device discovery");
            return [];
        }
    }

    private async Task PerformNetworkScanAsync(TimeSpan timeout, CancellationToken cancellationToken)
    {
        try
        {
            var localIPs = GetLocalNetworkAddresses();
            var tasks = new List<Task>();

            foreach (var localIP in localIPs)
            {
                var networkBase = GetNetworkBase(localIP);
                for (int i = 1; i < 255; i++)
                {
                    var ipToCheck = $"{networkBase}.{i}";
                    tasks.Add(CheckPlayStationDeviceAsync(ipToCheck, cancellationToken));
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
                logger.LogDebug("PlayStation discovery scan timed out");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during PlayStation network scan");
        }
    }

    private async Task CheckPlayStationDeviceAsync(string ipAddress, CancellationToken cancellationToken)
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
                            logger.LogInformation("Discovered PlayStation device: {DeviceName} at {IpAddress}", device.Name, device.IpAddress);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Could not check PlayStation device at {IpAddress}", ipAddress);
        }
    }

    private async Task<Zapper.Core.Models.Device?> DiscoverDeviceByIpAsync(string ipAddress, CancellationToken cancellationToken)
    {
        try
        {
            using var tcpClient = new TcpClient();
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(2));

            await tcpClient.ConnectAsync(ipAddress, RemotePlayPort).WaitAsync(timeoutCts.Token);

            if (tcpClient.Connected)
            {
                var device = new Zapper.Core.Models.Device
                {
                    Name = $"PlayStation ({ipAddress})",
                    Brand = "Sony",
                    Model = "PlayStation 4/5",
                    Type = DeviceType.PlayStation,
                    ConnectionType = ConnectionType.Network,
                    IpAddress = ipAddress,
                    Port = RemotePlayPort,
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
            logger.LogDebug(ex, "Failed to discover PlayStation device at {IpAddress}", ipAddress);
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
}