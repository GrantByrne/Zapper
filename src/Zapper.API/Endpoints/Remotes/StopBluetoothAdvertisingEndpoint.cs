using FastEndpoints;
using Microsoft.AspNetCore.SignalR;
using Zapper.Services;

namespace Zapper.API.Endpoints.Remotes;

public class StopBluetoothAdvertisingEndpoint(
    IBluetoothRemoteService bluetoothRemoteService,
    IHubContext<ZapperSignalR> hubContext) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("/api/remotes/bluetooth/advertise");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Stop advertising as a Bluetooth HID device";
            s.Description = "Stops the Pi from being discoverable as a Bluetooth remote";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            if (!bluetoothRemoteService.IsAdvertising)
            {
                await SendOkAsync(ct);
                return;
            }

            var success = await bluetoothRemoteService.StopAdvertisingAsync(ct);

            if (success)
            {
                await hubContext.Clients.All.SendAsync("BluetoothRemoteAdvertisingStopped",
                    cancellationToken: ct);
                await SendOkAsync(ct);
            }
            else
            {
                await SendErrorsAsync(500, ct);
            }
        }
        catch (Exception)
        {
            await SendErrorsAsync(500, ct);
        }
    }
}