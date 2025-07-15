using Microsoft.Extensions.DependencyInjection;
using Zapper.Core.Interfaces;

namespace Zapper.Device.Yamaha;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddYamahaDevice(this IServiceCollection services)
    {
        services.AddHttpClient<YamahaDeviceController>();
        services.AddHttpClient<YamahaDiscovery>();
        services.AddSingleton<IYamahaDeviceController, YamahaDeviceController>();
        services.AddSingleton<IYamahaDiscovery, YamahaDiscovery>();
        services.AddSingleton<IDeviceController, YamahaProtocolController>();

        return services;
    }
}