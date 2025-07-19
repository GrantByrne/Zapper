using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using MudBlazor;
using Zapper.Client;
using Zapper.Client.Devices;

namespace Zapper.Blazor.Components.AddDeviceWizard.Steps;

public partial class BluetoothScanStep(IZapperApiClient? apiClient, IJSRuntime jsRuntime) : BaseScanStep<string>, IAsyncDisposable
{
    protected override string ScanningMessage => "Scanning for available Bluetooth devices...";
    protected override string ScanningTitle => "Scanning for devices...";
    protected override string ScanningHelpText => "Please make sure your devices are in pairing mode";
    protected override bool ShowManualEntry => false;

    private HubConnection? _hubConnection;
    private string _selectedDevice = "";

    protected override async Task StartScanning()
    {
        if (apiClient == null)
        {
            ScanState.Error = "API client not available. Cannot scan for Bluetooth devices.";
            return;
        }

        try
        {
            ScanState.IsScanning = true;
            ScanState.Error = "";
            ScanState.DiscoveredDevices.Clear();
            _selectedDevice = "";
            StateHasChanged();

            await EnsureSignalRConnection();

            try
            {
                var scanRequest = new BluetoothScanRequest { DurationSeconds = 30 };
                var response = await apiClient.Devices.StartBluetoothScanAsync(scanRequest);

                if (!response.Success)
                {
                    ScanState.Error = response.Message ?? "Failed to start Bluetooth scan";
                    ScanState.IsScanning = false;
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                ScanState.Error = $"New scanning failed ({ex.Message}), using fallback method...";
                StateHasChanged();
                await Task.Delay(500);

                var devices = await apiClient.Devices.DiscoverBluetoothDevicesAsync();
                ScanState.DiscoveredDevices = devices.ToList();
                ScanState.IsScanning = false;
                StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            ScanState.Error = $"Failed to scan for Bluetooth devices: {ex.Message}";
            ScanState.IsScanning = false;
            StateHasChanged();
        }
    }

    protected override async Task StopScanning()
    {
        if (ScanState.IsScanning && apiClient != null)
        {
            try
            {
                var response = await apiClient.Devices.StopBluetoothScanAsync();
                if (response.Success)
                {
                    ScanState.IsScanning = false;
                    ScanState.Error = "";
                }
                else
                {
                    ScanState.Error = response.Message ?? "Failed to stop Bluetooth scan";
                    ScanState.IsScanning = false;
                }
                StateHasChanged();
            }
            catch (Exception ex)
            {
                ScanState.Error = $"Failed to stop Bluetooth scan: {ex.Message}";
                ScanState.IsScanning = false;
                StateHasChanged();
            }
        }
        else if (ScanState.IsScanning)
        {
            ScanState.IsScanning = false;
            ScanState.Error = "";
            StateHasChanged();
        }
    }

    protected override RenderFragment DeviceButtonContent(string device) => builder =>
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", "d-flex align-center");

        builder.OpenComponent<MudIcon>(2);
        builder.AddAttribute(3, "Icon", Icons.Material.Filled.Bluetooth);
        builder.AddAttribute(4, "Class", "mr-3");
        builder.CloseComponent();

        builder.OpenElement(5, "div");
        builder.AddAttribute(6, "class", "text-left");

        builder.OpenComponent<MudText>(7);
        builder.AddAttribute(8, "Typo", Typo.subtitle1);
        builder.AddContent(9, device);
        builder.CloseComponent();

        builder.OpenComponent<MudText>(10);
        builder.AddAttribute(11, "Typo", Typo.body2);
        builder.AddAttribute(12, "Class", "mud-text-secondary");
        builder.AddContent(13, "Bluetooth Device");
        builder.CloseComponent();

        builder.CloseElement();
        builder.CloseElement();
    };

    protected override bool IsDeviceSelected(string device) => _selectedDevice == device;

    protected override async Task SelectDevice(string device)
    {
        _selectedDevice = device;
        ScanState.SelectedDevice = device;

        var newDevice = new CreateDeviceRequest
        {
            Name = device,
            ConnectionType = Core.Models.ConnectionType.Bluetooth
        };

        await OnStepCompleted.InvokeAsync(newDevice);
    }

    private async Task EnsureSignalRConnection()
    {
        if (_hubConnection == null)
        {
            var baseUri = await jsRuntime.InvokeAsync<string>("eval", "window.location.origin");
            _hubConnection = new HubConnectionBuilder()
                .WithUrl($"{baseUri}/hubs/zapper")
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On("BluetoothScanStarted", () =>
            {
                InvokeAsync(() =>
                {
                    ScanState.IsScanning = true;
                    StateHasChanged();
                });
            });

            _hubConnection.On<object>("BluetoothDeviceFound", (device) =>
            {
                InvokeAsync(() =>
                {
                    var deviceName = device?.GetType().GetProperty("Name")?.GetValue(device)?.ToString();
                    if (!string.IsNullOrEmpty(deviceName) && !ScanState.DiscoveredDevices.Contains(deviceName))
                    {
                        ScanState.DiscoveredDevices.Add(deviceName);
                        StateHasChanged();
                    }
                });
            });

            _hubConnection.On("BluetoothScanCompleted", () =>
            {
                InvokeAsync(() =>
                {
                    ScanState.IsScanning = false;
                    StateHasChanged();
                });
            });

            _hubConnection.On<string>("BluetoothScanError", (error) =>
            {
                InvokeAsync(() =>
                {
                    ScanState.Error = error;
                    ScanState.IsScanning = false;
                    StateHasChanged();
                });
            });

            await _hubConnection.StartAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.DisposeAsync();
        }
    }
}