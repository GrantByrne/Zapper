using Microsoft.Extensions.Logging;
using Zapper.Core.Interfaces;
using Zapper.Core.Models;

namespace Zapper.Device.USB;

public class UsbDeviceController(IUsbRemoteHandler remoteHandler, ILogger<UsbDeviceController> logger) : IDeviceController
{
    public async Task<bool> SendCommandAsync(Zapper.Core.Models.Device device, DeviceCommand command)
    {
        if (!SupportsDevice(device))
        {
            logger.LogWarning("Device {DeviceId} is not supported by USB controller", device.Id);
            return false;
        }

        logger.LogWarning("USB devices are input-only. Cannot send command {CommandName} to device {DeviceName}", 
            command.Name, device.Name);
        
        return await Task.FromResult(false);
    }

    public Task<bool> TestConnectionAsync(Zapper.Core.Models.Device device)
    {
        if (!SupportsDevice(device))
            return Task.FromResult(false);

        var connectedRemotes = remoteHandler.GetConnectedRemotes();
        var isConnected = connectedRemotes.Any(remote => 
            remote.Contains(device.MacAddress ?? "") || 
            remote.Contains(device.Name));

        logger.LogDebug("USB device {DeviceName} connection test: {IsConnected}", 
            device.Name, isConnected);

        return Task.FromResult(isConnected);
    }

    public Task<DeviceStatus> GetStatusAsync(Zapper.Core.Models.Device device)
    {
        if (!SupportsDevice(device))
        {
            return Task.FromResult(new DeviceStatus
            {
                IsOnline = false,
                StatusMessage = "Device not supported by USB controller"
            });
        }

        var connectedRemotes = remoteHandler.GetConnectedRemotes();
        var isConnected = connectedRemotes.Any(remote => 
            remote.Contains(device.MacAddress ?? "") || 
            remote.Contains(device.Name));

        var status = new DeviceStatus
        {
            IsOnline = isConnected,
            StatusMessage = isConnected 
                ? $"USB remote connected via {device.MacAddress}" 
                : "USB remote not found"
        };

        if (isConnected)
        {
            status.Properties = new Dictionary<string, object>
            {
                ["IsListening"] = remoteHandler.IsListening,
                ["ConnectedRemotes"] = connectedRemotes.Count(),
                ["LastSeen"] = DateTime.UtcNow
            };
        }

        return Task.FromResult(status);
    }

    public bool SupportsDevice(Zapper.Core.Models.Device device)
    {
        return device.ConnectionType == ConnectionType.USB;
    }
}