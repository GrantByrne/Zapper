using MudBlazor;
using Zapper.Blazor.Components.AddDeviceWizard.Steps;
using Zapper.Core.Models;

namespace Zapper.Blazor.Components.AddDeviceWizard.DeviceTypes;

public class SonosDeviceDefinition : IDeviceTypeDefinition
{
    public ConnectionType ConnectionType => ConnectionType.NetworkHttp;
    public DeviceType? DeviceType => Core.Models.DeviceType.Sonos;
    public string DisplayName => "Sonos";
    public string Icon => Icons.Material.Filled.SpeakerGroup;
    public string Description => "Wireless Speakers";
    public Type ScanStepComponent => typeof(PlaceholderScanStep<Models.SonosDevice>);
    public WizardStep ScanStep => WizardStep.SonosScan;
}