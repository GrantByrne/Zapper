using Microsoft.Extensions.DependencyInjection;

namespace Zapper.Device.Network;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNetworkServices(this IServiceCollection services)
    {
        // Register HTTP client for network operations
        services.AddHttpClient<INetworkDeviceController, NetworkDeviceController>();

        // Register the network device controller
        services.AddSingleton<INetworkDeviceController, NetworkDeviceController>();

        return services;
    }
}