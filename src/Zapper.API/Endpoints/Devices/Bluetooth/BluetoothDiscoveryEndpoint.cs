using FastEndpoints;
using Zapper.Device.Bluetooth;

namespace Zapper.API.Endpoints.Devices.Bluetooth;

public class BluetoothDiscoveryEndpoint(
    AndroidTvBluetoothController androidTvController,
    AppleTvBluetoothController appleTvController) : EndpointWithoutRequest<IEnumerable<string>>
{
    public override void Configure()
    {
        Get("/api/devices/discover/bluetooth");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Discover Bluetooth devices";
            s.Description = "Get a list of paired Bluetooth devices that can be used for remote control (Android TV and Apple TV)";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var androidDevices = await androidTvController.DiscoverPairedDevices(ct);
        var appleDevices = await appleTvController.DiscoverPairedDevices(ct);

        var allDevices = androidDevices.Concat(appleDevices).Distinct();
        await SendOkAsync(allDevices, ct);
    }
}