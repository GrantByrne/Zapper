using Microsoft.Extensions.DependencyInjection;
using Zapper.Core.Interfaces;

namespace Zapper.Device.PlayStation;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlayStationDevice(this IServiceCollection services)
    {
        services.AddSingleton<IPlayStationDeviceController, PlayStationDeviceController>();
        services.AddSingleton<IPlayStationDiscovery, PlayStationDiscovery>();
        services.AddSingleton<IDeviceController, PlayStationProtocolController>();

        return services;
    }
}