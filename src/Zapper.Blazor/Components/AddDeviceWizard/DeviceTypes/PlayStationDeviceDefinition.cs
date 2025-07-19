using MudBlazor;
using Zapper.Blazor.Components.AddDeviceWizard.Steps;
using Zapper.Core.Models;

namespace Zapper.Blazor.Components.AddDeviceWizard.DeviceTypes;

public class PlayStationDeviceDefinition : IDeviceTypeDefinition
{
    public ConnectionType ConnectionType => ConnectionType.NetworkTcp;
    public DeviceType? DeviceType => Core.Models.DeviceType.PlayStation;
    public string DisplayName => "PlayStation";
    public string Icon => Icons.Material.Filled.SportsEsports;
    public string Description => "PS4, PS5 Consoles";
    public Type ScanStepComponent => typeof(PlaceholderScanStep<Models.PlayStationDevice>);
    public WizardStep ScanStep => WizardStep.PlayStationScan;
}