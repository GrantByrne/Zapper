using FastEndpoints;
using Microsoft.AspNetCore.SignalR;
using Zapper.Client.Remotes;
using Zapper.Services;

namespace Zapper.API.Endpoints.Remotes;

public class StartBluetoothAdvertisingEndpoint(
    IBluetoothRemoteService bluetoothRemoteService,
    IHubContext<ZapperSignalR> hubContext) : Endpoint<StartBluetoothAdvertisingRequest, StartBluetoothAdvertisingResponse>
{
    public override void Configure()
    {
        Post("/api/remotes/bluetooth/advertise");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Start advertising the Raspberry Pi as a Bluetooth HID device";
            s.Description = "Makes the Pi discoverable as a Bluetooth remote control that can be paired with TVs and other devices";
        });
    }

    public override async Task HandleAsync(StartBluetoothAdvertisingRequest req, CancellationToken ct)
    {
        try
        {
            if (bluetoothRemoteService.IsAdvertising)
            {
                await SendAsync(new StartBluetoothAdvertisingResponse
                {
                    Success = false,
                    ErrorMessage = "Bluetooth remote is already advertising",
                    IsAdvertising = true
                }, 400, ct);
                return;
            }

            if (!Enum.TryParse<RemoteDeviceType>(req.DeviceType, out var deviceType))
            {
                await SendAsync(new StartBluetoothAdvertisingResponse
                {
                    Success = false,
                    ErrorMessage = $"Invalid device type: {req.DeviceType}",
                    IsAdvertising = false
                }, 400, ct);
                return;
            }

            var success = await bluetoothRemoteService.StartAdvertisingAsync(req.RemoteName, deviceType, ct);

            if (success)
            {
                await hubContext.Clients.All.SendAsync("BluetoothRemoteAdvertisingStarted", new
                {
                    RemoteName = req.RemoteName,
                    DeviceType = req.DeviceType
                }, cancellationToken: ct);
            }

            await SendOkAsync(new StartBluetoothAdvertisingResponse
            {
                Success = success,
                ErrorMessage = success ? null : "Failed to start Bluetooth advertising",
                IsAdvertising = success
            }, ct);
        }
        catch (Exception ex)
        {
            await SendAsync(new StartBluetoothAdvertisingResponse
            {
                Success = false,
                ErrorMessage = ex.Message,
                IsAdvertising = false
            }, 500, ct);
        }
    }
}