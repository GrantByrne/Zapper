using Microsoft.Extensions.DependencyInjection;
using Zapper.Core.Interfaces;

namespace Zapper.Device.Sonos;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSonosDevice(this IServiceCollection services)
    {
        services.AddHttpClient<SonosDeviceController>();
        services.AddHttpClient<SonosDiscovery>();
        services.AddSingleton<ISonosDeviceController, SonosDeviceController>();
        services.AddSingleton<ISonosDiscovery, SonosDiscovery>();
        services.AddSingleton<IDeviceController, SonosProtocolController>();

        return services;
    }
}