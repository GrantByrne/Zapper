using Zapper.Core.Models;

namespace Zapper.Blazor.Components.AddDeviceWizard;

public interface IDeviceTypeDefinition
{
    ConnectionType ConnectionType { get; }
    DeviceType? DeviceType { get; }
    string DisplayName { get; }
    string Icon { get; }
    string Description { get; }
    Type ScanStepComponent { get; }
    WizardStep ScanStep { get; }
    bool RequiresIrCodeSelection => ConnectionType == ConnectionType.InfraredIr;
}