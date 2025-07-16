using FastEndpoints;
using Microsoft.AspNetCore.SignalR;
using Zapper.Client.Devices;
using Zapper.Device.Bluetooth;
using Zapper.Services;

namespace Zapper.API.Endpoints.Devices.Bluetooth;

public class BluetoothScanEndpoint(
    IBluetoothService bluetoothService,
    IHubContext<ZapperSignalR> hubContext) : Endpoint<BluetoothScanRequest, BluetoothScanResponse>
{
    public override void Configure()
    {
        Post("/api/devices/scan/bluetooth");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Start Bluetooth device scanning";
            s.Description = "Start scanning for discoverable Bluetooth devices and send real-time updates via SignalR";
        });
    }

    public override async Task HandleAsync(BluetoothScanRequest req, CancellationToken ct)
    {
        try
        {
            // Initialize Bluetooth service if needed
            if (!bluetoothService.IsInitialized)
            {
                await bluetoothService.Initialize(ct);
            }

            // Ensure Bluetooth is powered on
            if (!bluetoothService.IsPowered)
            {
                var powered = await bluetoothService.SetPowered(true, ct);
                if (!powered)
                {
                    await SendAsync(new BluetoothScanResponse
                    {
                        Success = false,
                        Message = "Failed to power on Bluetooth adapter",
                        IsScanning = false
                    }, 400, ct);
                    return;
                }
            }

            // Subscribe to device found events for SignalR updates
            bluetoothService.DeviceFound += OnDeviceFound;

            // Start discovery
            var started = await bluetoothService.StartDiscovery(ct);
            if (!started)
            {
                await SendAsync(new BluetoothScanResponse
                {
                    Success = false,
                    Message = "Failed to start Bluetooth discovery",
                    IsScanning = false
                }, 500, ct);
                return;
            }

            // Notify clients that scanning started
            await hubContext.Clients.All.SendAsync("BluetoothScanStarted", cancellationToken: ct);

            // Schedule discovery stop after duration
            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(req.DurationSeconds), ct);
                    await bluetoothService.StopDiscovery(ct);
                    bluetoothService.DeviceFound -= OnDeviceFound;
                    await hubContext.Clients.All.SendAsync("BluetoothScanCompleted", cancellationToken: ct);
                }
                catch (Exception ex)
                {
                    await hubContext.Clients.All.SendAsync("BluetoothScanError", ex.Message, cancellationToken: ct);
                }
            }, ct);

            await SendOkAsync(new BluetoothScanResponse
            {
                Success = true,
                Message = $"Bluetooth scanning started for {req.DurationSeconds} seconds",
                IsScanning = true
            }, ct);
        }
        catch (Exception ex)
        {
            await SendAsync(new BluetoothScanResponse
            {
                Success = false,
                Message = ex.Message,
                IsScanning = false
            }, 500, ct);
        }
    }

    private async void OnDeviceFound(object? sender, BluetoothDeviceEventArgs e)
    {
        // Send real-time device discovery updates via SignalR
        await hubContext.Clients.All.SendAsync("BluetoothDeviceFound", new
        {
            Name = e.Device.Name,
            Address = e.Device.Address,
            IsConnected = e.Device.IsConnected,
            IsPaired = e.Device.IsPaired
        });
    }
}