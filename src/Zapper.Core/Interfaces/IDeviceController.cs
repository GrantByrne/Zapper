using Zapper.Core.Models;

namespace Zapper.Core.Interfaces;

public interface IDeviceController
{
    Task<bool> SendCommandAsync(Device device, DeviceCommand command);
    Task<bool> TestConnectionAsync(Device device);
    Task<DeviceStatus> GetStatusAsync(Device device);
    bool SupportsDevice(Device device);
}