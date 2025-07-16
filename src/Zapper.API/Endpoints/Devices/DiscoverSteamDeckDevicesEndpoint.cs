using FastEndpoints;
using Zapper.Contracts.Devices;
using Zapper.Device.Bluetooth;

namespace Zapper.API.Endpoints.Devices;

public class DiscoverSteamDeckDevicesEndpoint(SteamDeckBluetoothController steamDeckController) : Endpoint<DiscoverSteamDeckDevicesRequest, IEnumerable<string>>
{

    public override void Configure()
    {
        Post("/api/devices/discover/steamdeck");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Discover paired Steam Deck devices";
            s.Description = "Get a list of paired Steam Deck devices available for Bluetooth control";
        });
    }

    public override async Task HandleAsync(DiscoverSteamDeckDevicesRequest req, CancellationToken ct)
    {
        var devices = await steamDeckController.DiscoverPairedDevices(ct);
        await SendOkAsync(devices, ct);
    }
}