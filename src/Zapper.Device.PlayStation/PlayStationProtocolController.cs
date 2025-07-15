using Microsoft.Extensions.Logging;
using Zapper.Core.Interfaces;

namespace Zapper.Device.PlayStation;

public class PlayStationProtocolController(IPlayStationDeviceController playStationController, ILogger<PlayStationProtocolController> logger) : IDeviceController
{
    public async Task<bool> SendCommandAsync(Zapper.Core.Models.Device device, Zapper.Core.Models.DeviceCommand command)
    {
        if (!SupportsDevice(device))
        {
            logger.LogWarning("Device {DeviceName} is not a PlayStation device", device.Name);
            return false;
        }

        return await playStationController.SendCommandAsync(device, command);
    }

    public async Task<bool> TestConnectionAsync(Zapper.Core.Models.Device device)
    {
        if (!SupportsDevice(device))
        {
            logger.LogWarning("Device {DeviceName} is not a PlayStation device", device.Name);
            return false;
        }

        return await playStationController.TestConnectionAsync(device);
    }

    public async Task<DeviceStatus> GetStatusAsync(Zapper.Core.Models.Device device)
    {
        if (!SupportsDevice(device))
        {
            return new DeviceStatus
            {
                IsOnline = false,
                StatusMessage = "Device is not a PlayStation"
            };
        }

        var isOnline = await playStationController.TestConnectionAsync(device);
        return new DeviceStatus
        {
            IsOnline = isOnline,
            StatusMessage = isOnline ? "PlayStation is online" : "PlayStation is offline"
        };
    }

    public bool SupportsDevice(Zapper.Core.Models.Device device)
    {
        return device.Type == Zapper.Core.Models.DeviceType.PlayStation &&
               device.ConnectionType == Zapper.Core.Models.ConnectionType.Network;
    }
}