using Zapper.Core.Models;

namespace Zapper.Core.Interfaces;

public interface IDeviceController
{
    Task<bool> SendCommand(Device device, DeviceCommand command);
    Task<bool> TestConnection(Device device);
    Task<DeviceStatus> GetStatus(Device device);
    bool SupportsDevice(Device device);
}