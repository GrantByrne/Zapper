using Microsoft.Extensions.Logging;
using Zapper.Core.Interfaces;

namespace Zapper.Device.Yamaha;

public class YamahaProtocolController(IYamahaDeviceController yamahaController, ILogger<YamahaProtocolController> logger) : IDeviceController
{
    public async Task<bool> SendCommandAsync(Zapper.Core.Models.Device device, Core.Models.DeviceCommand command)
    {
        if (!SupportsDevice(device))
        {
            logger.LogWarning("Device {DeviceName} is not a Yamaha receiver", device.Name);
            return false;
        }

        return await yamahaController.SendCommandAsync(device, command);
    }

    public async Task<bool> TestConnectionAsync(Zapper.Core.Models.Device device)
    {
        if (!SupportsDevice(device))
        {
            logger.LogWarning("Device {DeviceName} is not a Yamaha receiver", device.Name);
            return false;
        }

        return await yamahaController.TestConnectionAsync(device);
    }

    public async Task<DeviceStatus> GetStatusAsync(Zapper.Core.Models.Device device)
    {
        if (!SupportsDevice(device))
        {
            return new DeviceStatus
            {
                IsOnline = false,
                StatusMessage = "Device is not a Yamaha receiver"
            };
        }

        var isOnline = await yamahaController.TestConnectionAsync(device);
        return new DeviceStatus
        {
            IsOnline = isOnline,
            StatusMessage = isOnline ? "Yamaha receiver is online" : "Yamaha receiver is offline"
        };
    }

    public bool SupportsDevice(Zapper.Core.Models.Device device)
    {
        return device.Type == Core.Models.DeviceType.YamahaReceiver &&
               device.ConnectionType == Core.Models.ConnectionType.Network;
    }
}