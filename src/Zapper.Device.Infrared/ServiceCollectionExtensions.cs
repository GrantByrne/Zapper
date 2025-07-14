using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Zapper.Core.Interfaces;

namespace Zapper.Device.Infrared;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfraredServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Check if we're running on a platform that supports GPIO
        var useRealGpio = configuration.GetValue<bool>("Infrared:UseRealGpio", false);
        var gpioPin = configuration.GetValue<int>("Infrared:GpioPin", 18);
        
        if (useRealGpio)
        {
            services.AddSingleton<IInfraredTransmitter>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<GpioInfraredTransmitter>>();
                var transmitter = new GpioInfraredTransmitter(gpioPin, logger);
                transmitter.Initialize();
                return transmitter;
            });
        }
        else
        {
            services.AddSingleton<IInfraredTransmitter>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<MockInfraredTransmitter>>();
                var transmitter = new MockInfraredTransmitter(logger);
                transmitter.Initialize();
                return transmitter;
            });
        }
        
        // Register the device controller
        services.AddSingleton<IDeviceController, InfraredDeviceController>();
        
        return services;
    }
}