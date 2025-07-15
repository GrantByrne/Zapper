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

        services.AddSingleton<IBluetoothHidController, BluetoothHidController>();

        services.AddSingleton<IBluetoothDeviceController, AndroidTvBluetoothController>();
        services.AddSingleton<AndroidTvBluetoothController>();
        services.AddSingleton<IBluetoothDeviceController, AppleTvBluetoothController>();
        services.AddSingleton<AppleTvBluetoothController>();

        return services;
    }
}