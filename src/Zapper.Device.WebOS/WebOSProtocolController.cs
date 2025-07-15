using Microsoft.Extensions.Logging;
using Zapper.Core.Interfaces;
using Zapper.Core.Models;

namespace Zapper.Device.WebOS;

public class WebOsProtocolController(IWebOsDeviceController webOsController, ILogger<WebOsProtocolController> logger) : IDeviceController
{

    public async Task<bool> SendCommandAsync(Zapper.Core.Models.Device device, DeviceCommand command)
    {
        if (!SupportsDevice(device))
        {
            logger.LogWarning("Device {DeviceId} is not supported by WebOS controller", device.Id);
            return false;
        }

        try
        {
            var result = await webOsController.SendCommandAsync(device, command);

            if (command.DelayMs > 0)
            {
                await Task.Delay(command.DelayMs);
            }

            logger.LogDebug("Successfully sent WebOS command {CommandName} to device {DeviceName}",
                command.Name, device.Name);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send WebOS command {CommandName} to device {DeviceName}",
                command.Name, device.Name);
            return false;
        }
    }

    public async Task<bool> TestConnectionAsync(Zapper.Core.Models.Device device)
    {
        if (!SupportsDevice(device))
            return false;

        try
        {
            return await webOsController.TestConnectionAsync(device);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to test WebOS connection for device {DeviceName}", device.Name);
            return false;
        }
    }

    public async Task<DeviceStatus> GetStatusAsync(Zapper.Core.Models.Device device)
    {
        try
        {
            var isOnline = await TestConnectionAsync(device);
            return new DeviceStatus
            {
                IsOnline = isOnline,
                StatusMessage = isOnline ? "WebOS TV connected" : "WebOS TV not reachable"
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get WebOS device status for {DeviceName}", device.Name);
            return new DeviceStatus
            {
                IsOnline = false,
                StatusMessage = $"Error: {ex.Message}"
            };
        }
    }

    public bool SupportsDevice(Zapper.Core.Models.Device device)
    {
        return device.ConnectionType == ConnectionType.WebOs;
    }
}