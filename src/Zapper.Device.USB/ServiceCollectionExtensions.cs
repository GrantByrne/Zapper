using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Zapper.Core.Interfaces;

namespace Zapper.Device.USB;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUsbServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure USB settings
        services.Configure<UsbRemoteConfiguration>(config => configuration.GetSection("USB").Bind(config));

        var useMockHandler = configuration.GetValue<bool>("USB:UseMockHandler", true);

        if (useMockHandler)
        {
            services.AddSingleton<IUsbRemoteHandler>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<MockUsbRemoteHandler>>();
                return new MockUsbRemoteHandler(logger);
            });
        }
        else
        {
            services.AddSingleton<IUsbRemoteHandler>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<UsbRemoteHandler>>();
                var config = provider.GetRequiredService<IOptions<UsbRemoteConfiguration>>();
                return new UsbRemoteHandler(logger, config);
            });
        }

        // Register the device controller
        services.AddSingleton<IDeviceController, UsbDeviceController>();

        // Register as hosted service to manage lifecycle
        services.AddSingleton<IHostedService, UsbRemoteHostedService>();

        return services;
    }
}