using Microsoft.Extensions.Logging;
using Zapper.Core.Interfaces;
using Zapper.Core.Models;

namespace Zapper.Device.Infrared;

public class InfraredDeviceController(IInfraredTransmitter transmitter, ILogger<InfraredDeviceController> logger)
    : IDeviceController
{
    public async Task<bool> SendCommand(Zapper.Core.Models.Device device, DeviceCommand command)
    {
        if (!SupportsDevice(device))
        {
            logger.LogWarning("Device {DeviceId} is not supported by IR controller", device.Id);
            return false;
        }

        if (string.IsNullOrEmpty(command.IrCode))
        {
            logger.LogWarning("No IR code specified for command {CommandName}", command.Name);
            return false;
        }

        try
        {
            await transmitter.Transmit(command.IrCode, command.IsRepeatable ? 3 : 1);

            if (command.DelayMs > 0)
            {
                await Task.Delay(command.DelayMs);
            }

            logger.LogDebug("Successfully sent IR command {CommandName} to device {DeviceName}",
                command.Name, device.Name);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send IR command {CommandName} to device {DeviceName}",
                command.Name, device.Name);
            return false;
        }
    }

    public Task<bool> TestConnection(Zapper.Core.Models.Device device)
    {
        return Task.FromResult(transmitter.IsAvailable && SupportsDevice(device));
    }

    public Task<DeviceStatus> GetStatus(Zapper.Core.Models.Device device)
    {
        return Task.FromResult(new DeviceStatus
        {
            IsOnline = transmitter.IsAvailable,
            StatusMessage = transmitter.IsAvailable ? "IR transmitter ready" : "IR transmitter not available"
        });
    }

    public bool SupportsDevice(Zapper.Core.Models.Device device)
    {
        return device.ConnectionType == ConnectionType.InfraredIr;
    }
}