using Microsoft.Extensions.Logging;
using Zapper.Core.Interfaces;
using Zapper.Core.Models;

namespace Zapper.Integrations;

public class InfraredDeviceController : IDeviceController
{
    private readonly IInfraredTransmitter _transmitter;
    private readonly ILogger<InfraredDeviceController> _logger;

    public InfraredDeviceController(IInfraredTransmitter transmitter, ILogger<InfraredDeviceController> logger)
    {
        _transmitter = transmitter;
        _logger = logger;
    }

    public async Task<bool> SendCommandAsync(Device device, DeviceCommand command)
    {
        if (!SupportsDevice(device))
        {
            _logger.LogWarning("Device {DeviceId} is not supported by IR controller", device.Id);
            return false;
        }

        if (string.IsNullOrEmpty(command.IrCode))
        {
            _logger.LogWarning("No IR code specified for command {CommandName}", command.Name);
            return false;
        }

        try
        {
            await _transmitter.TransmitAsync(command.IrCode, command.IsRepeatable ? 3 : 1);
            
            if (command.DelayMs > 0)
            {
                await Task.Delay(command.DelayMs);
            }

            _logger.LogDebug("Successfully sent IR command {CommandName} to device {DeviceName}", 
                command.Name, device.Name);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send IR command {CommandName} to device {DeviceName}", 
                command.Name, device.Name);
            return false;
        }
    }

    public Task<bool> TestConnectionAsync(Device device)
    {
        return Task.FromResult(_transmitter.IsAvailable && SupportsDevice(device));
    }

    public Task<DeviceStatus> GetStatusAsync(Device device)
    {
        return Task.FromResult(new DeviceStatus
        {
            IsOnline = _transmitter.IsAvailable,
            StatusMessage = _transmitter.IsAvailable ? "IR transmitter ready" : "IR transmitter not available"
        });
    }

    public bool SupportsDevice(Device device)
    {
        return device.ConnectionType == ConnectionType.InfraredIR;
    }
}