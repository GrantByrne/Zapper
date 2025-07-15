using Microsoft.Extensions.Logging;
using Zapper.Core.Interfaces;

namespace Zapper.Device.Sonos;

public class SonosProtocolController(ISonosDeviceController sonosController, ILogger<SonosProtocolController> logger) : IDeviceController
{
    public async Task<bool> SendCommandAsync(Zapper.Core.Models.Device device, Core.Models.DeviceCommand command)
    {
        if (!SupportsDevice(device))
        {
            logger.LogWarning("Device {DeviceName} is not a Sonos device", device.Name);
            return false;
        }

        return await sonosController.SendCommandAsync(device, command);
    }

    public async Task<bool> TestConnectionAsync(Zapper.Core.Models.Device device)
    {
        if (!SupportsDevice(device))
        {
            logger.LogWarning("Device {DeviceName} is not a Sonos device", device.Name);
            return false;
        }

        return await sonosController.TestConnectionAsync(device);
    }

    public async Task<DeviceStatus> GetStatusAsync(Zapper.Core.Models.Device device)
    {
        if (!SupportsDevice(device))
        {
            return new DeviceStatus
            {
                IsOnline = false,
                StatusMessage = "Device is not a Sonos speaker"
            };
        }

        var isOnline = await sonosController.TestConnectionAsync(device);
        return new DeviceStatus
        {
            IsOnline = isOnline,
            StatusMessage = isOnline ? "Sonos speaker is online" : "Sonos speaker is offline"
        };
    }

    public bool SupportsDevice(Zapper.Core.Models.Device device)
    {
        return device.Type == Core.Models.DeviceType.Sonos &&
               device.ConnectionType == Core.Models.ConnectionType.Network;
    }
}