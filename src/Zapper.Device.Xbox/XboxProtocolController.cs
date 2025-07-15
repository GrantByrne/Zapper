using Microsoft.Extensions.Logging;
using Zapper.Core.Interfaces;

namespace Zapper.Device.Xbox;

public class XboxProtocolController(IXboxDeviceController xboxController, ILogger<XboxProtocolController> logger) : IDeviceController
{
    public async Task<bool> SendCommandAsync(Zapper.Core.Models.Device device, Zapper.Core.Models.DeviceCommand command)
    {
        if (!SupportsDevice(device))
        {
            logger.LogWarning("Device {DeviceName} is not an Xbox device", device.Name);
            return false;
        }

        return await xboxController.SendCommandAsync(device, command);
    }

    public async Task<bool> TestConnectionAsync(Zapper.Core.Models.Device device)
    {
        if (!SupportsDevice(device))
        {
            logger.LogWarning("Device {DeviceName} is not an Xbox device", device.Name);
            return false;
        }

        return await xboxController.TestConnectionAsync(device);
    }

    public async Task<DeviceStatus> GetStatusAsync(Zapper.Core.Models.Device device)
    {
        if (!SupportsDevice(device))
        {
            return new DeviceStatus
            {
                IsOnline = false,
                StatusMessage = "Device is not an Xbox"
            };
        }

        var isOnline = await xboxController.TestConnectionAsync(device);
        return new DeviceStatus
        {
            IsOnline = isOnline,
            StatusMessage = isOnline ? "Xbox is online" : "Xbox is offline"
        };
    }

    public bool SupportsDevice(Zapper.Core.Models.Device device)
    {
        return device.Type == Core.Models.DeviceType.Xbox &&
               device.ConnectionType == Core.Models.ConnectionType.Network;
    }
}