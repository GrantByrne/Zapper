using MudBlazor;
using Zapper.Blazor.Components.AddDeviceWizard.Steps;
using Zapper.Core.Models;

namespace Zapper.Blazor.Components.AddDeviceWizard.DeviceTypes;

public class WebOsDeviceDefinition : IDeviceTypeDefinition
{
    public ConnectionType ConnectionType => ConnectionType.WebOs;
    public DeviceType? DeviceType => Core.Models.DeviceType.SmartTv;
    public string DisplayName => "WebOS";
    public string Icon => Icons.Material.Filled.Tv;
    public string Description => "LG Smart TVs";
    public Type ScanStepComponent => typeof(WebOsScanStep);
    public WizardStep ScanStep => WizardStep.WebOsScan;
}