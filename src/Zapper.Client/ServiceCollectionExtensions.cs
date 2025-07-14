using Microsoft.Extensions.DependencyInjection;
using Refit;
using Zapper.Client.Abstractions;

namespace Zapper.Client;

/// <summary>
/// Extension methods for registering Zapper API client services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add Zapper API client services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">Client configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddZapperApiClient(
        this IServiceCollection services, 
        ZapperClientConfiguration configuration)
    {
        // Register configuration
        services.AddSingleton(configuration);
        
        // Register Refit API interfaces
        services.AddRefitClient<IDeviceApi>()
            .ConfigureHttpClient(c => 
            {
                c.BaseAddress = new Uri(configuration.BaseUrl);
                c.Timeout = TimeSpan.FromSeconds(configuration.TimeoutSeconds);
            });
        
        // Register client implementations
        services.AddScoped<IDeviceClient, DeviceClient>();
        services.AddScoped<IZapperApiClient, ZapperApiClient>();
        
        return services;
    }
    
    /// <summary>
    /// Add Zapper API client services with default configuration
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="baseUrl">Base URL for the API (optional, defaults to localhost:5000)</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddZapperApiClient(
        this IServiceCollection services, 
        string? baseUrl = null)
    {
        var configuration = new ZapperClientConfiguration();
        if (!string.IsNullOrEmpty(baseUrl))
        {
            configuration.BaseUrl = baseUrl;
        }
        
        return services.AddZapperApiClient(configuration);
    }
}