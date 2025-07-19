using MudBlazor;
using Zapper.Blazor.Components.AddDeviceWizard.Steps;
using Zapper.Core.Models;

namespace Zapper.Blazor.Components.AddDeviceWizard.DeviceTypes;

public class DenonDeviceDefinition : IDeviceTypeDefinition
{
    public ConnectionType ConnectionType => ConnectionType.Network;
    public DeviceType? DeviceType => Core.Models.DeviceType.DenonReceiver;
    public string DisplayName => "Denon/Marantz";
    public string Icon => Icons.Material.Filled.Speaker;
    public string Description => "AVR Network Control";
    public Type ScanStepComponent => typeof(PlaceholderScanStep<Models.DenonDevice>);
    public WizardStep ScanStep => WizardStep.DenonScan;
}