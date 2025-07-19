using MudBlazor;
using Zapper.Blazor.Components.AddDeviceWizard.Steps;
using Zapper.Core.Models;

namespace Zapper.Blazor.Components.AddDeviceWizard.DeviceTypes;

public class InfraredDeviceDefinition : IDeviceTypeDefinition
{
    public ConnectionType ConnectionType => ConnectionType.InfraredIr;
    public DeviceType? DeviceType => null; // Multiple device types can use IR
    public string DisplayName => "Infrared";
    public string Icon => Icons.Material.Filled.Sensors;
    public string Description => "TVs, Sound Bars, Cable Boxes";
    public Type ScanStepComponent => typeof(IrCodeSelectionStep); // Goes directly to IR code selection
    public WizardStep ScanStep => WizardStep.IrCodeSelection;
}