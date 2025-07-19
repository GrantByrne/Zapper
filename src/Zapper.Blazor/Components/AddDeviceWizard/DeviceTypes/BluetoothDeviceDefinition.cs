using MudBlazor;
using Zapper.Blazor.Components.AddDeviceWizard.Steps;
using Zapper.Core.Models;

namespace Zapper.Blazor.Components.AddDeviceWizard.DeviceTypes;

public class BluetoothDeviceDefinition : IDeviceTypeDefinition
{
    public ConnectionType ConnectionType => ConnectionType.Bluetooth;
    public DeviceType? DeviceType => null; // Multiple device types can use Bluetooth
    public string DisplayName => "Bluetooth";
    public string Icon => Icons.Material.Filled.Bluetooth;
    public string Description => "Apple TV, Android TV, Controllers";
    public Type ScanStepComponent => typeof(BluetoothScanStep);
    public WizardStep ScanStep => WizardStep.BluetoothScan;
}