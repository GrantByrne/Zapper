using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Zapper.Core.Interfaces;

namespace Zapper.Device.Denon;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDenonDevice(this IServiceCollection services)
    {
        services.AddScoped<IDenonDiscovery, DenonDiscovery>();
        services.AddHttpClient<DenonDeviceController>()
            .ConfigureHttpClient((client) =>
            {
                client.Timeout = TimeSpan.FromSeconds(10);
            });

        services.AddScoped<IDenonDeviceController>(provider =>
        {
            var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(nameof(DenonDeviceController));
            var logger = provider.GetRequiredService<ILogger<DenonDeviceController>>();
            return new DenonDeviceController(httpClient, logger);
        });

        services.AddScoped<DenonProtocolController>();
        services.AddScoped<IDeviceController, DenonProtocolController>();

        return services;
    }
}