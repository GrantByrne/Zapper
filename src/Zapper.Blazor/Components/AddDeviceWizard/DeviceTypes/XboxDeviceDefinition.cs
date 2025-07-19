using MudBlazor;
using Zapper.Blazor.Components.AddDeviceWizard.Steps;
using Zapper.Core.Models;

namespace Zapper.Blazor.Components.AddDeviceWizard.DeviceTypes;

public class XboxDeviceDefinition : IDeviceTypeDefinition
{
    public ConnectionType ConnectionType => ConnectionType.NetworkTcp;
    public DeviceType? DeviceType => Core.Models.DeviceType.Xbox;
    public string DisplayName => "Xbox";
    public string Icon => Icons.Material.Filled.VideogameAsset;
    public string Description => "Xbox One, Series X/S";
    public Type ScanStepComponent => typeof(PlaceholderScanStep<Models.XboxDevice>);
    public WizardStep ScanStep => WizardStep.XboxScan;
}