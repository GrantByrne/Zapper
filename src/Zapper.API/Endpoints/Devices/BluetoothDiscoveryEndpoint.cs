using FastEndpoints;
using Zapper.Device.Bluetooth;

namespace Zapper.Endpoints.Devices;

public class BluetoothDiscoveryEndpoint(IBluetoothDeviceController bluetoothController) : EndpointWithoutRequest<IEnumerable<string>>
{
    public override void Configure()
    {
        Get("/api/devices/discover/bluetooth");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Discover Bluetooth devices";
            s.Description = "Get a list of paired Bluetooth devices that can be used for remote control";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var devices = await bluetoothController.DiscoverPairedDevicesAsync(ct);
        await SendOkAsync(devices, ct);
    }
}