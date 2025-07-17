using Microsoft.Extensions.DependencyInjection;
using Zapper.Device.AppleTV.Controllers;
using Zapper.Device.AppleTV.Services;

namespace Zapper.Device.AppleTV.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAppleTvSupport(this IServiceCollection services)
    {
        services.AddScoped<AppleTvDiscoveryService>();
        services.AddScoped<AppleTvControllerFactory>();
        services.AddScoped<AppleTvService>();

        services.AddScoped<CompanionProtocolController>();
        services.AddScoped<MrpProtocolController>();

        return services;
    }
}