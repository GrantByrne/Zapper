using Microsoft.AspNetCore.Components;
using Zapper.Client.Devices;

namespace Zapper.Blazor.Components.AddDeviceWizard.Steps;

public partial class DeviceTypeSelectionStep : ComponentBase, IWizardStep
{
    [Inject] private IDeviceTypeRegistry DeviceTypeRegistry { get; set; } = default!;

    [Parameter] public EventCallback<CreateDeviceRequest> OnStepCompleted { get; set; }
    [Parameter] public EventCallback OnPreviousStep { get; set; }
    [Parameter] public EventCallback<IDeviceTypeDefinition> OnDeviceTypeSelected { get; set; }

    private IEnumerable<IDeviceTypeDefinition> DeviceTypes => DeviceTypeRegistry.GetAllDeviceTypes();
    private IDeviceTypeDefinition? _selectedDeviceType;

    public Task InitializeAsync() => Task.CompletedTask;

    private async Task SelectDeviceType(IDeviceTypeDefinition deviceType)
    {
        _selectedDeviceType = deviceType;
        await OnDeviceTypeSelected.InvokeAsync(deviceType);
    }

    private string GetCardClass(IDeviceTypeDefinition deviceType)
    {
        bool isSelected = _selectedDeviceType == deviceType;
        return $"device-type-card {(isSelected ? "selected" : "")}";
    }
}