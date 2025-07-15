using FastEndpoints;
using Microsoft.AspNetCore.SignalR;
using Zapper.Device.Bluetooth;
using Zapper.Services;

namespace Zapper.API.Endpoints.Devices;

public class StopBluetoothScanEndpoint(
    IBluetoothService bluetoothService,
    IHubContext<ZapperSignalR> hubContext) : EndpointWithoutRequest<StopBluetoothScanResponse>
{
    public override void Configure()
    {
        Post("/api/devices/scan/bluetooth/stop");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Stop Bluetooth device scanning";
            s.Description = "Stop any ongoing Bluetooth device discovery";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            if (!bluetoothService.IsInitialized)
            {
                await SendAsync(new StopBluetoothScanResponse
                {
                    Success = false,
                    Message = "Bluetooth service not initialized"
                }, 400, ct);
                return;
            }

            var stopped = await bluetoothService.StopDiscovery(ct);

            if (stopped)
            {
                await hubContext.Clients.All.SendAsync("BluetoothScanCompleted", cancellationToken: ct);
            }

            await SendOkAsync(new StopBluetoothScanResponse
            {
                Success = stopped,
                Message = stopped ? "Bluetooth scanning stopped" : "No active Bluetooth scan to stop"
            }, ct);
        }
        catch (Exception ex)
        {
            await SendAsync(new StopBluetoothScanResponse
            {
                Success = false,
                Message = ex.Message
            }, 500, ct);
        }
    }
}