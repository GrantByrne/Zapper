using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Zapper.Device.Bluetooth;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBluetoothServices(this IServiceCollection services)
    {
        // Register the Bluetooth service as both interface and hosted service
        services.AddSingleton<IBluetoothService, BluetoothService>();
        services.AddSingleton<IHostedService>(provider => 
            (BluetoothService)provider.GetRequiredService<IBluetoothService>());
        
        // Register HID controller
        services.AddSingleton<IBluetoothHIDController, BluetoothHidController>();
        
        // Register device-specific controllers
        services.AddSingleton<IBluetoothDeviceController, AndroidTVBluetoothController>();
        services.AddSingleton<AndroidTVBluetoothController>();
        
        return services;
    }
}