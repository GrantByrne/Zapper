using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using MudBlazor;
using Zapper.Blazor.Components.AddDeviceWizard.Models;
using Zapper.Client;
using Zapper.Client.Devices;

namespace Zapper.Blazor.Components.AddDeviceWizard.Steps;

public partial class WebOsScanStep(IZapperApiClient? apiClient, IJSRuntime jsRuntime) : BaseScanStep<WebOsDeviceModel>, IAsyncDisposable
{
    protected override string ScanningMessage => "Scanning for WebOS TVs on your network...";
    protected override string ScanningTitle => "Scanning for LG TVs...";
    protected override string ScanningHelpText => "Please ensure your TV is powered on and connected to the network";
    protected override bool ShowManualEntry => true;

    private HubConnection? _hubConnection;
    private WebOsDeviceModel? _selectedDevice;

    protected override async Task StartScanning()
    {
        if (apiClient == null)
        {
            ScanState.Error = "API client not available. Cannot scan for WebOS TVs.";
            return;
        }

        try
        {
            ScanState.IsScanning = true;
            ScanState.Error = "";
            ScanState.DiscoveredDevices.Clear();
            _selectedDevice = null;
            ScanState.ManualIpAddress = "";
            StateHasChanged();

            await EnsureSignalRConnection();

            try
            {
                var scanRequest = new WebOsScanRequest { DurationSeconds = 15 };
                var response = await apiClient.Devices.StartWebOsScanAsync(scanRequest);

                if (!response.Success)
                {
                    ScanState.Error = response.Message ?? "Failed to start WebOS TV scan";
                    ScanState.IsScanning = false;
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                ScanState.Error = $"Failed to start WebOS TV scan: {ex.Message}";
                ScanState.IsScanning = false;
                StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            ScanState.Error = $"Failed to scan for WebOS TVs: {ex.Message}";
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
                var response = await apiClient.Devices.StopWebOsScanAsync();
                if (response.Success)
                {
                    ScanState.IsScanning = false;
                    ScanState.Error = "";
                }
                else
                {
                    ScanState.Error = response.Message ?? "Failed to stop WebOS scan";
                    ScanState.IsScanning = false;
                }
                StateHasChanged();
            }
            catch (Exception ex)
            {
                ScanState.Error = $"Failed to stop WebOS scan: {ex.Message}";
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

    protected override RenderFragment DeviceButtonContent(WebOsDeviceModel device) => builder =>
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", "d-flex align-center");

        builder.OpenComponent<MudIcon>(2);
        builder.AddAttribute(3, "Icon", Icons.Material.Filled.Tv);
        builder.AddAttribute(4, "Class", "mr-3");
        builder.CloseComponent();

        builder.OpenElement(5, "div");
        builder.AddAttribute(6, "class", "text-left");

        builder.OpenComponent<MudText>(7);
        builder.AddAttribute(8, "Typo", Typo.subtitle1);
        builder.AddContent(9, device.Name);
        builder.CloseComponent();

        builder.OpenComponent<MudText>(10);
        builder.AddAttribute(11, "Typo", Typo.body2);
        builder.AddAttribute(12, "Class", "mud-text-secondary");
        builder.AddContent(13, $"{device.ModelName ?? "LG TV"} - {device.IpAddress}");
        builder.CloseComponent();

        builder.CloseElement();
        builder.CloseElement();
    };

    protected override bool IsDeviceSelected(WebOsDeviceModel device) => _selectedDevice == device;

    protected override async Task SelectDevice(WebOsDeviceModel device)
    {
        _selectedDevice = device;
        ScanState.SelectedDevice = device;

        var newDevice = new CreateDeviceRequest
        {
            Name = device.Name,
            IpAddress = device.IpAddress,
            Brand = "LG",
            Model = device.ModelName ?? "",
            ConnectionType = Core.Models.ConnectionType.WebOs,
            Type = Core.Models.DeviceType.SmartTv
        };

        await OnStepCompleted.InvokeAsync(newDevice);
    }

    protected override async Task OnManualIpKeyUp(Microsoft.AspNetCore.Components.Web.KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !string.IsNullOrWhiteSpace(ScanState.ManualIpAddress))
        {
            var newDevice = new CreateDeviceRequest
            {
                Name = $"WebOS TV ({ScanState.ManualIpAddress.Trim()})",
                IpAddress = ScanState.ManualIpAddress.Trim(),
                Brand = "LG",
                ConnectionType = Core.Models.ConnectionType.WebOs,
                Type = Core.Models.DeviceType.SmartTv
            };

            await OnStepCompleted.InvokeAsync(newDevice);
        }
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

            _hubConnection.On("WebOSScanStarted", () =>
            {
                InvokeAsync(() =>
                {
                    ScanState.IsScanning = true;
                    StateHasChanged();
                });
            });

            _hubConnection.On<object>("WebOSDeviceFound", (device) =>
            {
                InvokeAsync(() =>
                {
                    var deviceType = device?.GetType();
                    var name = deviceType?.GetProperty("Name")?.GetValue(device)?.ToString();
                    var ipAddress = deviceType?.GetProperty("IpAddress")?.GetValue(device)?.ToString();
                    var modelName = deviceType?.GetProperty("ModelName")?.GetValue(device)?.ToString();
                    var modelNumber = deviceType?.GetProperty("ModelNumber")?.GetValue(device)?.ToString();
                    var port = deviceType?.GetProperty("Port")?.GetValue(device)?.ToString();

                    if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(ipAddress))
                    {
                        var webOsDevice = new WebOsDeviceModel
                        {
                            Name = name,
                            IpAddress = ipAddress,
                            ModelName = modelName,
                            ModelNumber = modelNumber,
                            Port = !string.IsNullOrEmpty(port) ? int.Parse(port) : 3000
                        };

                        if (!ScanState.DiscoveredDevices.Any(d => d.IpAddress == webOsDevice.IpAddress))
                        {
                            ScanState.DiscoveredDevices.Add(webOsDevice);
                            StateHasChanged();
                        }
                    }
                });
            });

            _hubConnection.On("WebOSScanCompleted", () =>
            {
                InvokeAsync(() =>
                {
                    ScanState.IsScanning = false;
                    StateHasChanged();
                });
            });

            _hubConnection.On<string>("WebOSScanError", (error) =>
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