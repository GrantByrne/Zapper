using Microsoft.AspNetCore.Components;
using Zapper.Client.Devices;

namespace Zapper.Blazor.Components.AddDeviceWizard.Steps;

public partial class DeviceConfigurationStep : ComponentBase, IWizardStep
{
    [Parameter] public EventCallback<CreateDeviceRequest> OnStepCompleted { get; set; }
    [Parameter] public EventCallback OnPreviousStep { get; set; }
    [Parameter] public CreateDeviceRequest Device { get; set; } = new();

    public Task InitializeAsync() => Task.CompletedTask;

    protected override async Task OnParametersSetAsync()
    {
        if (string.IsNullOrEmpty(Device.Name) && !string.IsNullOrEmpty(Device.Brand))
        {
            Device.Name = $"{Device.Brand} {Device.Type}";
        }
        await base.OnParametersSetAsync();
    }
}