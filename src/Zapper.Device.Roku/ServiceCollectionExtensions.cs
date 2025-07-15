using Microsoft.Extensions.DependencyInjection;

namespace Zapper.Device.Roku;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRokuServices(this IServiceCollection services)
    {
        services.AddSingleton<IRokuDeviceController, RokuDeviceController>();
        services.AddSingleton<IRokuDiscovery, RokuDiscovery>();

        return services;
    }
}