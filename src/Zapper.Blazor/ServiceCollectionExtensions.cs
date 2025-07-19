using Microsoft.Extensions.DependencyInjection;
using Zapper.Blazor.Components.AddDeviceWizard;
using Zapper.Blazor.Components.AddDeviceWizard.DeviceTypes;

namespace Zapper.Blazor;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDeviceWizard(this IServiceCollection services)
    {
        // Register device type definitions
        services.AddSingleton<IDeviceTypeDefinition, BluetoothDeviceDefinition>();
        services.AddSingleton<IDeviceTypeDefinition, InfraredDeviceDefinition>();
        services.AddSingleton<IDeviceTypeDefinition, WebOsDeviceDefinition>();
        services.AddSingleton<IDeviceTypeDefinition, PlayStationDeviceDefinition>();
        services.AddSingleton<IDeviceTypeDefinition, XboxDeviceDefinition>();
        services.AddSingleton<IDeviceTypeDefinition, RokuDeviceDefinition>();
        services.AddSingleton<IDeviceTypeDefinition, YamahaDeviceDefinition>();
        services.AddSingleton<IDeviceTypeDefinition, SonosDeviceDefinition>();

        // Register the device type registry
        services.AddSingleton<IDeviceTypeRegistry, DeviceTypeRegistry>();

        return services;
    }
}