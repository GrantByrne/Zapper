using Microsoft.EntityFrameworkCore;
using Zapper.Core.Interfaces;
using Zapper.Data;
using Zapper.Integrations;
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

builder.Services.AddTransient<IWebOSClient>(provider =>
{
    var logger = provider.GetRequiredService<ILogger<WebOSClient>>();
    return new WebOSClient(logger);
});

builder.Services.AddSingleton<IWebOSDiscovery>(provider =>
{
    var logger = provider.GetRequiredService<ILogger<WebOSDiscovery>>();
    var webOSClient = provider.GetRequiredService<IWebOSClient>();
    return new WebOSDiscovery(logger, webOSClient);
});

builder.Services.AddTransient<IWebOSDeviceController>(provider =>
{
    var logger = provider.GetRequiredService<ILogger<WebOSHardwareController>>();
    var client = provider.GetRequiredService<IWebOSClient>();
    return new WebOSHardwareController(client, logger);
});

// Register protocol implementations
builder.Services.AddTransient<InfraredDeviceController>();
builder.Services.AddTransient<WebOSProtocolController>();

// Register device controllers with factory pattern
builder.Services.AddTransient<IDeviceController>(provider =>
{
    // This is a simplified factory - in a real implementation, you'd choose based on device type
    return provider.GetRequiredService<InfraredDeviceController>();
});

// Register business services
builder.Services.AddScoped<IDeviceService, DeviceService>();
builder.Services.AddScoped<IActivityService, ActivityService>();
builder.Services.AddScoped<IIRCodeService, IRCodeService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// Add SignalR
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Map SignalR hub
app.MapHub<ZapperSignalR>("/hubs/zapper");

// Serve the main page
app.MapGet("/", () => Results.Redirect("/index.html"));

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ZapperContext>();
    context.Database.EnsureCreated();
}

app.Run();