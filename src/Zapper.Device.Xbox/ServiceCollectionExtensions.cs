using Microsoft.Extensions.DependencyInjection;
using Zapper.Core.Interfaces;

namespace Zapper.Device.Xbox;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddXboxDevice(this IServiceCollection services)
    {
        services.AddSingleton<IXboxDiscovery, XboxDiscovery>();
        services.AddSingleton<IXboxDeviceController, XboxDeviceController>();
        services.AddSingleton<IDeviceController, XboxProtocolController>();

        return services;
    }
}