using Microsoft.Extensions.Logging;
using Zapper.Core.Interfaces;
using Zapper.Core.Models;

namespace Zapper.Device.WebOS;

public class WebOSProtocolController : IDeviceController
{
    private readonly IWebOSDeviceController _webOSController;
    private readonly ILogger<WebOSProtocolController> _logger;

    public WebOSProtocolController(IWebOSDeviceController webOSController, ILogger<WebOSProtocolController> logger)
    {
        _webOSController = webOSController;
        _logger = logger;
    }

    public async Task<bool> SendCommandAsync(Zapper.Core.Models.Device device, DeviceCommand command)
    {
        if (!SupportsDevice(device))
        {
            _logger.LogWarning("Device {DeviceId} is not supported by WebOS controller", device.Id);
            return false;
        }

        try
        {
            var result = await _webOSController.SendCommandAsync(device, command);
            
            if (command.DelayMs > 0)
            {
                await Task.Delay(command.DelayMs);
            }

            _logger.LogDebug("Successfully sent WebOS command {CommandName} to device {DeviceName}", 
                command.Name, device.Name);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send WebOS command {CommandName} to device {DeviceName}", 
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
            return await _webOSController.TestConnectionAsync(device);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to test WebOS connection for device {DeviceName}", device.Name);
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
            _logger.LogError(ex, "Failed to get WebOS device status for {DeviceName}", device.Name);
            return new DeviceStatus
            {
                IsOnline = false,
                StatusMessage = $"Error: {ex.Message}"
            };
        }
    }

    public bool SupportsDevice(Zapper.Core.Models.Device device)
    {
        return device.ConnectionType == ConnectionType.WebOS;
    }
}