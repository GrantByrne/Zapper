using MudBlazor;
using Zapper.Blazor.Components.AddDeviceWizard.Steps;
using Zapper.Core.Models;

namespace Zapper.Blazor.Components.AddDeviceWizard.DeviceTypes;

public class RokuDeviceDefinition : IDeviceTypeDefinition
{
    public ConnectionType ConnectionType => ConnectionType.NetworkHttp;
    public DeviceType? DeviceType => Core.Models.DeviceType.StreamingDevice;
    public string DisplayName => "Roku";
    public string Icon => Icons.Material.Filled.Cast;
    public string Description => "Roku Streaming Devices";
    public Type ScanStepComponent => typeof(PlaceholderScanStep<Models.RokuDevice>);
    public WizardStep ScanStep => WizardStep.RokuScan;
}