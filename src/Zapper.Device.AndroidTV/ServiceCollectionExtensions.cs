using Microsoft.Extensions.DependencyInjection;
using Zapper.Core.Interfaces;

namespace Zapper.Device.AndroidTV;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAndroidTvAdbSupport(this IServiceCollection services)
    {
        services.AddTransient<IAdbClient, AdbClient>();
        services.AddTransient<IDeviceController, AndroidTvAdbController>();
        services.AddTransient<AndroidTvAdbController>();
        services.AddTransient<AdbDiscoveryService>();

        return services;
    }
}