using Zapper.Core.Models;

namespace Zapper.Core.Interfaces;

public interface IDeviceController
{
    Task<bool> SendCommandAsync(Device device, DeviceCommand command);
    Task<bool> TestConnectionAsync(Device device);
    Task<DeviceStatus> GetStatusAsync(Device device);
    bool SupportsDevice(Device device);
}

public class DeviceStatus
{
    public bool IsOnline { get; set; }
    public string? StatusMessage { get; set; }
    public Dictionary<string, object> Properties { get; set; } = new();
}