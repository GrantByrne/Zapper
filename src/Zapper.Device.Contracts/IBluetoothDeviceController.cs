using Zapper.Core.Models;

namespace Zapper.Device.Contracts;

public interface IBluetoothDeviceController
{
    Task<bool> SendCommandAsync(Zapper.Core.Models.Device device, DeviceCommand command, CancellationToken cancellationToken = default);
    Task<bool> TestConnectionAsync(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> DiscoverPairedDevicesAsync(CancellationToken cancellationToken = default);
}