using MudBlazor;
using Zapper.Blazor.Components.AddDeviceWizard.Steps;
using Zapper.Core.Models;

namespace Zapper.Blazor.Components.AddDeviceWizard.DeviceTypes;

public class YamahaDeviceDefinition : IDeviceTypeDefinition
{
    public ConnectionType ConnectionType => ConnectionType.NetworkHttp;
    public DeviceType? DeviceType => Core.Models.DeviceType.Receiver;
    public string DisplayName => "Yamaha";
    public string Icon => Icons.Material.Filled.Speaker;
    public string Description => "MusicCast Receivers";
    public Type ScanStepComponent => typeof(PlaceholderScanStep<Models.YamahaDevice>);
    public WizardStep ScanStep => WizardStep.YamahaScan;
}