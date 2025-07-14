using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Zapper.Device.Bluetooth;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBluetoothServices(this IServiceCollection services)
    {
        services.AddSingleton<IBluetoothService, BluetoothService>();
        services.AddSingleton<IHostedService>(provider => 
            (BluetoothService)provider.GetRequiredService<IBluetoothService>());
        
        services.AddSingleton<IBluetoothHIDController, BluetoothHidController>();
        
        services.AddSingleton<IBluetoothDeviceController, AndroidTVBluetoothController>();
        services.AddSingleton<AndroidTVBluetoothController>();
        services.AddSingleton<IBluetoothDeviceController, AppleTVBluetoothController>();
        services.AddSingleton<AppleTVBluetoothController>();
        
        return services;
    }
}