using Microsoft.Extensions.DependencyInjection;
using Zapper.Core.Interfaces;
using Zapper.Device.Xbox.Network;

namespace Zapper.Device.Xbox;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddXboxDevice(this IServiceCollection services)
    {
        services.AddSingleton<INetworkClientFactory, NetworkClientFactory>();
        services.AddSingleton<IXboxDiscovery, XboxDiscovery>();
        services.AddSingleton<IXboxDeviceController, XboxDeviceController>();
        services.AddSingleton<IDeviceController, XboxProtocolController>();

        return services;
    }
}