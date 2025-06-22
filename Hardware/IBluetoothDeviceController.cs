using ZapperHub.Models;

namespace ZapperHub.Hardware;

public interface IBluetoothDeviceController
{
    Task<bool> SendCommandAsync(Device device, DeviceCommand command, CancellationToken cancellationToken = default);
    Task<bool> TestConnectionAsync(Device device, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> DiscoverPairedDevicesAsync(CancellationToken cancellationToken = default);
}