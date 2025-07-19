using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.EntityFrameworkCore;
using Zapper.API.Endpoints.Devices;
using Zapper.API.Endpoints.Devices.Bluetooth;
using Zapper.Core.Interfaces;
using Zapper.Data;
using Zapper.Device.Infrared;
using Zapper.Device.Network;
using Zapper.Device.WebOS;
using Zapper.Device.Roku;
using Zapper.Device.USB;
using Zapper.Device.Bluetooth;
using Zapper.Device.Xbox;
using Zapper.Device.PlayStation;
using Zapper.Device.Sonos;
using Zapper.Device.Yamaha;
using Zapper.Device.AppleTV.Extensions;
using Zapper.Device.AndroidTV;
using Zapper.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddZapperDatabase();

// Register settings service first (needed by hardware services)
builder.Services.AddSingleton<ISettingsService, SettingsService>();

// Register hardware abstractions
builder.Services.AddSingleton<IInfraredTransmitter>(provider =>
{
    var logger = provider.GetRequiredService<ILogger<GpioInfraredTransmitter>>();
    var settingsService = provider.GetRequiredService<ISettingsService>();

    // Get settings to determine GPIO configuration
    var settings = settingsService.GetSettingsAsync().GetAwaiter().GetResult();

    // Use mock transmitter if GPIO is disabled
    if (!settings.Hardware.EnableGpio)
    {
        return new MockInfraredTransmitter(provider.GetRequiredService<ILogger<MockInfraredTransmitter>>());
    }

    var gpioPin = settings.Hardware.Infrared.TransmitterGpioPin;
    return new GpioInfraredTransmitter(gpioPin, logger);
});

builder.Services.AddSingleton<IInfraredReceiver>(provider =>
{
    var logger = provider.GetRequiredService<ILogger<GpioInfraredReceiver>>();
    var settingsService = provider.GetRequiredService<ISettingsService>();

    // Get settings to determine GPIO configuration
    var settings = settingsService.GetSettingsAsync().GetAwaiter().GetResult();

    // Use mock receiver if GPIO is disabled
    if (!settings.Hardware.EnableGpio)
    {
        return new MockInfraredReceiver(provider.GetRequiredService<ILogger<MockInfraredReceiver>>());
    }

    var gpioPin = settings.Hardware.Infrared.ReceiverGpioPin;
    return new GpioInfraredReceiver(gpioPin, logger);
});

builder.Services.AddSingleton<IUsbRemoteHandler>(provider =>
{
    var logger = provider.GetRequiredService<ILogger<UsbRemoteHandler>>();
    return new UsbRemoteHandler(logger);
});

// Register USB remote hosted services
builder.Services.AddHostedService<UsbRemoteHostedService>();
builder.Services.AddHostedService<UsbRemoteEventHandler>();

builder.Services.AddTransient<IWebOsClient>(provider =>
{
    var logger = provider.GetRequiredService<ILogger<WebOsClient>>();
    return new WebOsClient(logger);
});

builder.Services.AddSingleton<IWebOsDiscovery>(provider =>
{
    var logger = provider.GetRequiredService<ILogger<WebOsDiscovery>>();
    var webOsClient = provider.GetRequiredService<IWebOsClient>();
    return new WebOsDiscovery(logger, webOsClient);
});

builder.Services.AddTransient<IWebOsDeviceController>(provider =>
{
    var logger = provider.GetRequiredService<ILogger<WebOsHardwareController>>();
    var client = provider.GetRequiredService<IWebOsClient>();
    return new WebOsHardwareController(client, logger);
});

// Register HttpClient for network operations
builder.Services.AddHttpClient();

// Register network device controller
builder.Services.AddTransient<INetworkDeviceController, NetworkDeviceController>();

// Register Roku services
builder.Services.AddRokuServices();

// Register Bluetooth services
builder.Services.AddBluetoothServices();

// Register Xbox services
builder.Services.AddXboxDevice();

// Register PlayStation services
builder.Services.AddPlayStationDevice();

// Register Sonos services
builder.Services.AddSonosDevice();

// Register Yamaha services
builder.Services.AddYamahaDevice();

// Register Apple TV services
builder.Services.AddAppleTvSupport();

// Register Android TV ADB services
builder.Services.AddAndroidTvAdbSupport();

// Register protocol implementations
builder.Services.AddTransient<InfraredDeviceController>();
builder.Services.AddTransient<WebOsProtocolController>();

// Register device controllers with factory pattern
builder.Services.AddTransient<IDeviceController>(provider =>
{
    // This is a simplified factory - in a real implementation, you'd choose based on device type
    return provider.GetRequiredService<InfraredDeviceController>();
});

// Register business services
builder.Services.AddScoped<IDeviceService, DeviceService>();
builder.Services.AddScoped<IActivityService, ActivityService>();
builder.Services.AddScoped<IIrCodeService, IrCodeService>();
builder.Services.AddScoped<IExternalIrCodeService, IrdbService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IIrLearningService, IrLearningService>();
builder.Services.AddSingleton<IIrTroubleshootingService, IrTroubleshootingService>();
builder.Services.AddScoped<IUsbRemoteService, UsbRemoteService>();
builder.Services.AddSingleton<IBluetoothRemoteService, BluetoothRemoteService>();
builder.Services.AddScoped<ISystemDiagnosticsService, SystemDiagnosticsService>();

// Add SignalR
builder.Services.AddSignalR();

// Add FastEndpoints
builder.Services.AddFastEndpoints(o =>
{
    o.Assemblies = [typeof(BluetoothControlEndpoint).Assembly];
});

// Add Swagger documentation
builder.Services.SwaggerDocument();

// Add Blazor WebAssembly hosting
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerGen();
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

// Use FastEndpoints
app.UseFastEndpoints();

// Map SignalR hub
app.MapHub<ZapperSignalR>("/hubs/zapper");

// Map Blazor fallback
app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

// Ensure database is created and migrations are applied
await app.Services.EnsureDatabaseAsync();

app.Run();