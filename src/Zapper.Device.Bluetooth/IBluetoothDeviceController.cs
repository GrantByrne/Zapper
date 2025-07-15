using Zapper.Core.Models;

namespace Zapper.Device.Bluetooth;

public interface IBluetoothDeviceController
{
    Task<bool> SendCommand(Zapper.Core.Models.Device device, DeviceCommand command, CancellationToken cancellationToken = default);
    Task<bool> TestConnection(Zapper.Core.Models.Device device, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> DiscoverPairedDevices(CancellationToken cancellationToken = default);
}