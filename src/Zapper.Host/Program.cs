using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.EntityFrameworkCore;
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
using Zapper.Endpoints.Devices;
using Zapper.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddDbContext<ZapperContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register hardware abstractions
builder.Services.AddSingleton<IInfraredTransmitter>(provider =>
{
    var logger = provider.GetRequiredService<ILogger<GpioInfraredTransmitter>>();
    var config = provider.GetRequiredService<IConfiguration>();

    // Use mock transmitter if GPIO is disabled
    if (!config.GetValue<bool>("Hardware:EnableGPIO", true))
    {
        return new MockInfraredTransmitter(provider.GetRequiredService<ILogger<MockInfraredTransmitter>>());
    }

    var gpioPin = config.GetValue<int>("Hardware:IRTransmitter:GpioPin", 18);
    return new GpioInfraredTransmitter(gpioPin, logger);
});

builder.Services.AddSingleton<IUsbRemoteHandler>(provider =>
{
    var logger = provider.GetRequiredService<ILogger<UsbRemoteHandler>>();
    return new UsbRemoteHandler(logger);
});

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

// Register protocol implementations
builder.Services.AddTransient<Zapper.Device.Infrared.InfraredDeviceController>();
builder.Services.AddTransient<Zapper.Device.WebOS.WebOsProtocolController>();

// Register device controllers with factory pattern
builder.Services.AddTransient<IDeviceController>(provider =>
{
    // This is a simplified factory - in a real implementation, you'd choose based on device type
    return provider.GetRequiredService<Zapper.Device.Infrared.InfraredDeviceController>();
});

// Register business services
builder.Services.AddScoped<IDeviceService, DeviceService>();
builder.Services.AddScoped<IActivityService, ActivityService>();
builder.Services.AddScoped<IIrCodeService, IrCodeService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

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

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ZapperContext>();
    context.Database.EnsureCreated();
}

app.Run();