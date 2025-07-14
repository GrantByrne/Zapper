using Microsoft.Extensions.DependencyInjection;

namespace Zapper.API;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddZapperApi(this IServiceCollection services)
    {
        // FastEndpoints will automatically discover and register all endpoints in this assembly
        return services;
    }
}