using Microsoft.AspNetCore.Components;
using Zapper.Client.Devices;
using Zapper.Core.Models;

namespace Zapper.Blazor.Components.AddDeviceWizard.Steps;

public partial class IrCodeSelectionStep : ComponentBase, IWizardStep
{
    [Parameter] public EventCallback<CreateDeviceRequest> OnStepCompleted { get; set; }
    [Parameter] public EventCallback OnPreviousStep { get; set; }

    private IrCodeSet? _selectedIrCodeSet;

    public Task InitializeAsync() => Task.CompletedTask;

    private async Task HandleIrCodeSetSelected(IrCodeSet codeSet)
    {
        _selectedIrCodeSet = codeSet;

        var newDevice = new CreateDeviceRequest
        {
            Brand = codeSet.Brand,
            Model = codeSet.Model,
            IrCodeSetId = codeSet.Id,
            ConnectionType = ConnectionType.InfraredIr
        };

        await OnStepCompleted.InvokeAsync(newDevice);
    }
}