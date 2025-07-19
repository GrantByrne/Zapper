using Microsoft.AspNetCore.Components;
using Zapper.Client.Devices;

namespace Zapper.Blazor.Components.AddDeviceWizard.Steps;

public partial class PlaceholderScanStep<TDevice> : BaseScanStep<TDevice> where TDevice : class
{
    [Parameter] public string DeviceTypeName { get; set; } = "Device";

    protected override string ScanningMessage => $"Scanning for {DeviceTypeName} devices...";
    protected override string ScanningTitle => "Scanning for devices...";
    protected override string ScanningHelpText => $"Please ensure your {DeviceTypeName} is powered on and connected to the network";

    protected override Task StartScanning()
    {
        ScanState.Error = "This device type is not yet implemented.";
        ScanState.IsScanning = false;
        StateHasChanged();
        return Task.CompletedTask;
    }

    protected override Task StopScanning() => Task.CompletedTask;

    protected override RenderFragment DeviceButtonContent(TDevice device) => builder => { };

    protected override bool IsDeviceSelected(TDevice device) => false;

    protected override Task SelectDevice(TDevice device) => Task.CompletedTask;
}