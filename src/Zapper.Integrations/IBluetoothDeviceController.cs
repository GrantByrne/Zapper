using Zapper.Core.Models;

namespace Zapper.Integrations;

public interface IBluetoothDeviceController
{
    Task<bool> SendCommandAsync(Device device, DeviceCommand command, CancellationToken cancellationToken = default);
    Task<bool> TestConnectionAsync(Device device, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> DiscoverPairedDevicesAsync(CancellationToken cancellationToken = default);
}