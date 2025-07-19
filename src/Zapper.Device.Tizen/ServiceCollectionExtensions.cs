using Microsoft.Extensions.DependencyInjection;
using Zapper.Core.Interfaces;

namespace Zapper.Device.Tizen;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTizenDevice(this IServiceCollection services)
    {
        services.AddSingleton<ITizenClient, TizenClient>();
        services.AddSingleton<ITizenDeviceController, TizenHardwareController>();
        services.AddSingleton<ITizenDiscovery, TizenDiscovery>();
        services.AddSingleton<IDeviceController, TizenProtocolController>();

        return services;
    }
}