using Microsoft.Extensions.Logging;
using Zapper.Core.Interfaces;

namespace Zapper.Device.PlayStation;

public class PlayStationProtocolController(IPlayStationDeviceController playStationController, ILogger<PlayStationProtocolController> logger) : IDeviceController
{
    public async Task<bool> SendCommand(Zapper.Core.Models.Device device, Core.Models.DeviceCommand command)
    {
        if (!SupportsDevice(device))
        {
            logger.LogWarning("Device {DeviceName} is not a PlayStation device", device.Name);
            return false;
        }

        return await playStationController.SendCommand(device, command);
    }

    public async Task<bool> TestConnection(Zapper.Core.Models.Device device)
    {
        if (!SupportsDevice(device))
        {
            logger.LogWarning("Device {DeviceName} is not a PlayStation device", device.Name);
            return false;
        }

        return await playStationController.TestConnection(device);
    }

    public async Task<DeviceStatus> GetStatus(Zapper.Core.Models.Device device)
    {
        if (!SupportsDevice(device))
        {
            return new DeviceStatus
            {
                IsOnline = false,
                StatusMessage = "Device is not a PlayStation"
            };
        }

        var isOnline = await playStationController.TestConnection(device);
        return new DeviceStatus
        {
            IsOnline = isOnline,
            StatusMessage = isOnline ? "PlayStation is online" : "PlayStation is offline"
        };
    }

    public bool SupportsDevice(Zapper.Core.Models.Device device)
    {
        return device.Type == Core.Models.DeviceType.PlayStation &&
               device.ConnectionType == Core.Models.ConnectionType.Network;
    }
}