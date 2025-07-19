using Microsoft.Extensions.Logging;
using Zapper.Core.Interfaces;
using Zapper.Core.Models;

namespace Zapper.Device.Tizen;

public class TizenProtocolController(ITizenDeviceController tizenController, ILogger<TizenProtocolController> logger) : IDeviceController
{
    public async Task<bool> SendCommand(Zapper.Core.Models.Device device, DeviceCommand command)
    {
        if (!SupportsDevice(device))
        {
            logger.LogWarning("Device {DeviceId} is not supported by Tizen controller", device.Id);
            return false;
        }

        try
        {
            var result = await tizenController.SendCommand(device, command);

            if (command.DelayMs > 0)
            {
                await Task.Delay(command.DelayMs);
            }

            logger.LogDebug("Successfully sent Tizen command {CommandName} to device {DeviceName}",
                command.Name, device.Name);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send Tizen command {CommandName} to device {DeviceName}",
                command.Name, device.Name);
            return false;
        }
    }

    public async Task<bool> TestConnection(Zapper.Core.Models.Device device)
    {
        if (!SupportsDevice(device))
            return false;

        try
        {
            return await tizenController.TestConnection(device);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to test Tizen connection for device {DeviceName}", device.Name);
            return false;
        }
    }

    public async Task<DeviceStatus> GetStatus(Zapper.Core.Models.Device device)
    {
        try
        {
            var isOnline = await TestConnection(device);
            return new DeviceStatus
            {
                IsOnline = isOnline,
                StatusMessage = isOnline ? "Samsung Tizen TV connected" : "Samsung Tizen TV not reachable"
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get Tizen device status for {DeviceName}", device.Name);
            return new DeviceStatus
            {
                IsOnline = false,
                StatusMessage = $"Error: {ex.Message}"
            };
        }
    }

    public bool SupportsDevice(Zapper.Core.Models.Device device)
    {
        return device.ConnectionType == ConnectionType.Tizen;
    }
}