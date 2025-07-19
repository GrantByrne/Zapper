using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Zapper.Device.AndroidTV.Models;

namespace Zapper.Device.AndroidTV;

public class AdbDiscoveryService(ILogger<AdbDiscoveryService> logger, ILoggerFactory loggerFactory) : IAdbDiscoveryService
{
    private readonly ILogger<AdbDiscoveryService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ILoggerFactory _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));

    public async Task<IEnumerable<AdbDevice>> DiscoverDevicesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting ADB device discovery");

        var devices = new List<AdbDevice>();
        var tasks = new List<Task>();

        var localIpAddresses = GetLocalIpAddresses();

        foreach (var localIp in localIpAddresses)
        {
            var networkPrefix = GetNetworkPrefix(localIp);
            if (networkPrefix != null)
            {
                for (int i = 1; i <= 254; i++)
                {
                    var targetIp = $"{networkPrefix}.{i}";

                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            var device = await TryDiscoverDevice(targetIp, cancellationToken);
                            if (device != null)
                            {
                                lock (devices)
                                {
                                    devices.Add(device);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogDebug(ex, "Failed to check device at {IpAddress}", targetIp);
                        }
                    }, cancellationToken));
                }
            }
        }

        await Task.WhenAll(tasks);

        _logger.LogInformation("ADB discovery completed. Found {DeviceCount} devices", devices.Count);
        return devices;
    }

    public async Task<bool> TestDeviceAsync(string host, int port = 5555, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = new TcpClient();
            var connectTask = client.ConnectAsync(host, port);
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

            var completedTask = await Task.WhenAny(connectTask, timeoutTask);

            if (completedTask == connectTask && client.Connected)
            {
                using var adbClient = new AdbClient(_loggerFactory.CreateLogger<AdbClient>());
                return await adbClient.ConnectAsync(host, port, cancellationToken);
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to test ADB connection to {Host}:{Port}", host, port);
            return false;
        }
    }

    private async Task<AdbDevice?> TryDiscoverDevice(string ipAddress, CancellationToken cancellationToken)
    {
        if (!await IsHostReachable(ipAddress, cancellationToken))
            return null;

        if (!await TestDeviceAsync(ipAddress, 5555, cancellationToken))
            return null;

        try
        {
            using var adbClient = new AdbClient(_loggerFactory.CreateLogger<AdbClient>());
            var connected = await adbClient.ConnectAsync(ipAddress, 5555, cancellationToken);

            if (connected)
            {
                var deviceName = await adbClient.ExecuteShellCommandWithResponseAsync("getprop ro.product.model", cancellationToken) ?? "Unknown Android Device";
                var manufacturer = await adbClient.ExecuteShellCommandWithResponseAsync("getprop ro.product.manufacturer", cancellationToken) ?? "Unknown";
                var androidVersion = await adbClient.ExecuteShellCommandWithResponseAsync("getprop ro.build.version.release", cancellationToken) ?? "Unknown";

                return new AdbDevice
                {
                    IpAddress = ipAddress,
                    Port = 5555,
                    Name = deviceName.Trim(),
                    Manufacturer = manufacturer.Trim(),
                    AndroidVersion = androidVersion.Trim(),
                    IsAndroidTv = await IsAndroidTvDevice(adbClient, cancellationToken)
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to get device info from {IpAddress}", ipAddress);
        }

        return null;
    }

    private async Task<bool> IsHostReachable(string ipAddress, CancellationToken cancellationToken)
    {
        try
        {
            using var ping = new Ping();
            var reply = await ping.SendPingAsync(ipAddress, 1000);
            return reply.Status == IPStatus.Success;
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> IsAndroidTvDevice(IAdbClient adbClient, CancellationToken cancellationToken)
    {
        try
        {
            var characteristics = await adbClient.ExecuteShellCommandWithResponseAsync("getprop ro.build.characteristics", cancellationToken);
            var packageManager = await adbClient.ExecuteShellCommandWithResponseAsync("pm list features | grep android.software.leanback", cancellationToken);

            return characteristics?.Contains("tv") == true || !string.IsNullOrEmpty(packageManager);
        }
        catch
        {
            return false;
        }
    }

    private static IEnumerable<IPAddress> GetLocalIpAddresses()
    {
        var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

        foreach (var networkInterface in networkInterfaces)
        {
            if (networkInterface.OperationalStatus != OperationalStatus.Up ||
                networkInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                continue;

            var ipProperties = networkInterface.GetIPProperties();

            foreach (var unicastAddress in ipProperties.UnicastAddresses)
            {
                if (unicastAddress.Address.AddressFamily == AddressFamily.InterNetwork &&
                    !IPAddress.IsLoopback(unicastAddress.Address))
                {
                    yield return unicastAddress.Address;
                }
            }
        }
    }

    private static string? GetNetworkPrefix(IPAddress ipAddress)
    {
        var addressBytes = ipAddress.GetAddressBytes();
        if (addressBytes.Length == 4)
        {
            return $"{addressBytes[0]}.{addressBytes[1]}.{addressBytes[2]}";
        }
        return null;
    }
}