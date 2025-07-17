using Microsoft.Extensions.Logging;
using Zapper.Core.Models;
using Zapper.Device.AppleTV.Models;
using Zeroconf;

namespace Zapper.Device.AppleTV.Services;

public class AppleTvDiscoveryService(ILogger<AppleTvDiscoveryService> logger)
{
    private readonly string[] _serviceTypes =
    [
        "_companion-link._tcp.local.",
        "_mediaremotetv._tcp.local.",
        "_airplay._tcp.local.",
        "_dacp._tcp.local."
    ];

    public async Task<List<AppleTvDevice>> DiscoverDevicesAsync(int timeoutSeconds = 5)
    {
        var devices = new List<AppleTvDevice>();

        try
        {
            var responses = await ZeroconfResolver.ResolveAsync(_serviceTypes, TimeSpan.FromSeconds(timeoutSeconds));

            foreach (var response in responses)
            {
                foreach (var service in response.Services)
                {
                    var device = CreateDeviceFromService(response, service.Value);
                    if (device != null)
                    {
                        devices.Add(device);
                        logger.LogInformation("Discovered Apple TV: {Name} at {IpAddress}:{Port}",
                            device.Name, device.IpAddress, device.Port);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error discovering Apple TV devices");
        }

        return devices.DistinctBy(d => d.Identifier).ToList();
    }

    private AppleTvDevice? CreateDeviceFromService(IZeroconfHost host, IService service)
    {
        try
        {
            var device = new AppleTvDevice
            {
                Name = host.DisplayName ?? "Unknown Apple TV",
                IpAddress = host.IPAddress,
                Port = service.Port,
                Services = [service.Name]
            };

            if (service.Properties != null && service.Properties.Count > 0)
            {
                var properties = service.Properties[0];
                foreach (var kvp in properties)
                {
                    device.Properties[kvp.Key] = kvp.Value;

                    switch (kvp.Key.ToLower())
                    {
                        case "deviceid":
                        case "deviceidentifier":
                            device.Identifier = kvp.Value;
                            break;
                        case "model":
                            device.Model = kvp.Value;
                            break;
                        case "osvers":
                        case "systemversion":
                            device.Version = kvp.Value;
                            break;
                        case "rpmd":
                            device.RequiresPairing = kvp.Value == "1";
                            break;
                    }
                }
            }

            device.PreferredProtocol = DeterminePreferredProtocol(service.Name, device.Version);

            return device;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating device from service {ServiceName}", service.Name);
            return null;
        }
    }

    private ConnectionType DeterminePreferredProtocol(string serviceName, string version)
    {
        if (serviceName.Contains("companion-link"))
            return ConnectionType.CompanionProtocol;

        if (serviceName.Contains("mediaremotetv"))
        {
            if (!string.IsNullOrEmpty(version) && int.TryParse(version.Split('.')[0], out var majorVersion))
            {
                return majorVersion >= 15 ? ConnectionType.CompanionProtocol : ConnectionType.MediaRemoteProtocol;
            }
            return ConnectionType.MediaRemoteProtocol;
        }

        if (serviceName.Contains("airplay"))
            return ConnectionType.AirPlay;

        if (serviceName.Contains("dacp"))
            return ConnectionType.Dacp;

        return ConnectionType.MediaRemoteProtocol;
    }
}