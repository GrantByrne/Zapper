using Microsoft.AspNetCore.Components;
using Zapper.Client.Devices;

namespace Zapper.Blazor.Components.AddDeviceWizard;

public interface IWizardStep
{
    EventCallback<CreateDeviceRequest> OnStepCompleted { get; set; }
    EventCallback OnPreviousStep { get; set; }
    Task InitializeAsync();
}