using Microsoft.AspNetCore.Components;
using MudBlazor;
using Zapper.Blazor.Components.AddDeviceWizard.Steps;
using Zapper.Client.Devices;

namespace Zapper.Blazor.Components.AddDeviceWizard;

public partial class AddDeviceWizard : ComponentBase
{
    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public EventCallback<bool> IsVisibleChanged { get; set; }
    [Parameter] public EventCallback<CreateDeviceRequest> OnDeviceAdded { get; set; }

    private WizardStep _currentStep = WizardStep.DeviceType;
    private IDeviceTypeDefinition? _selectedDeviceType;
    private CreateDeviceRequest _newDevice = new();
    private DialogOptions _dialogOptions = new() { MaxWidth = MaxWidth.Medium, FullWidth = true };

    private Type GetCurrentStepComponent()
    {
        return _currentStep switch
        {
            WizardStep.DeviceType => typeof(DeviceTypeSelectionStep),
            WizardStep.Configuration => typeof(DeviceConfigurationStep),
            _ => _selectedDeviceType?.ScanStepComponent ?? typeof(DeviceTypeSelectionStep)
        };
    }

    private Dictionary<string, object> GetStepParameters()
    {
        var parameters = new Dictionary<string, object>
        {
            { nameof(IWizardStep.OnPreviousStep), EventCallback.Factory.Create(this, PreviousStep) }
        };

        if (_currentStep == WizardStep.DeviceType)
        {
            parameters.Add("OnDeviceTypeSelected", EventCallback.Factory.Create<IDeviceTypeDefinition>(this, HandleDeviceTypeSelected));
        }
        else if (_currentStep == WizardStep.Configuration)
        {
            parameters.Add("Device", _newDevice);
            parameters.Add(nameof(IWizardStep.OnStepCompleted), EventCallback.Factory.Create<CreateDeviceRequest>(this, HandleConfigurationCompleted));
        }
        else
        {
            parameters.Add(nameof(IWizardStep.OnStepCompleted), EventCallback.Factory.Create<CreateDeviceRequest>(this, HandleScanCompleted));
        }

        return parameters;
    }

    private string GetWizardTitle()
    {
        return _currentStep switch
        {
            WizardStep.DeviceType => "Add New Device - Select Type",
            WizardStep.Configuration => $"Add New Device - Configure {_selectedDeviceType?.DisplayName ?? "Device"}",
            WizardStep.IrCodeSelection => "Add New Device - Select IR Codes",
            _ => $"Add New Device - Scan for {_selectedDeviceType?.DisplayName ?? "Device"}"
        };
    }

    private void HandleDeviceTypeSelected(IDeviceTypeDefinition deviceType)
    {
        _selectedDeviceType = deviceType;
        _newDevice.ConnectionType = deviceType.ConnectionType;

        if (deviceType.DeviceType.HasValue)
        {
            _newDevice.Type = deviceType.DeviceType.Value;
        }

        _currentStep = deviceType.ScanStep;
        StateHasChanged();
    }

    private void HandleScanCompleted(CreateDeviceRequest device)
    {
        _newDevice = device ?? _newDevice;
        _currentStep = WizardStep.Configuration;
        StateHasChanged();
    }

    private async Task HandleConfigurationCompleted(CreateDeviceRequest device)
    {
        _newDevice = device;
        await FinishWizard();
    }

    private void PreviousStep()
    {
        if (_currentStep == WizardStep.Configuration)
        {
            _currentStep = _selectedDeviceType?.ScanStep ?? WizardStep.DeviceType;
        }
        else
        {
            _currentStep = WizardStep.DeviceType;
            _selectedDeviceType = null;
        }
        StateHasChanged();
    }

    private async Task FinishWizard()
    {
        if (!string.IsNullOrWhiteSpace(_newDevice.Name))
        {
            await OnDeviceAdded.InvokeAsync(_newDevice);
            ResetWizard();
        }
    }

    private void ResetWizard()
    {
        _currentStep = WizardStep.DeviceType;
        _selectedDeviceType = null;
        _newDevice = new CreateDeviceRequest();
        IsVisible = false;
        _ = IsVisibleChanged.InvokeAsync(false);
    }

    private void CancelWizard()
    {
        ResetWizard();
    }

    protected override void OnParametersSet()
    {
        if (!IsVisible && _currentStep != WizardStep.DeviceType)
        {
            ResetWizard();
        }
    }
}