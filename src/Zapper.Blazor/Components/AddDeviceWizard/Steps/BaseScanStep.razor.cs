using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Zapper.Client.Devices;

namespace Zapper.Blazor.Components.AddDeviceWizard.Steps;

public abstract partial class BaseScanStep<TDevice> : ComponentBase, IWizardStep where TDevice : class
{
    [Parameter] public EventCallback<CreateDeviceRequest> OnStepCompleted { get; set; }
    [Parameter] public EventCallback OnPreviousStep { get; set; }

    protected DeviceScanState<TDevice> ScanState { get; set; } = new();

    protected abstract string ScanningMessage { get; }
    protected abstract string ScanningTitle { get; }
    protected abstract string ScanningHelpText { get; }
    protected virtual bool ShowManualEntry => true;

    protected abstract Task StartScanning();
    protected abstract Task StopScanning();
    protected abstract RenderFragment DeviceButtonContent(TDevice device);
    protected abstract bool IsDeviceSelected(TDevice device);
    protected abstract Task SelectDevice(TDevice device);

    public virtual async Task InitializeAsync()
    {
        await StartScanning();
    }

    protected async Task RetryScanning()
    {
        ScanState.Error = "";
        ScanState.DiscoveredDevices.Clear();
        ScanState.SelectedDevice = null;
        ScanState.ManualIpAddress = "";
        await StartScanning();
    }

    protected virtual async Task OnManualIpKeyUp(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !string.IsNullOrWhiteSpace(ScanState.ManualIpAddress))
        {
            ScanState.SelectedDevice = null;
            await OnStepCompleted.InvokeAsync(null);
        }
    }

    public override async Task SetParametersAsync(ParameterView parameters)
    {
        await base.SetParametersAsync(parameters);
    }

    public virtual void Dispose()
    {
        _ = StopScanning();
    }
}