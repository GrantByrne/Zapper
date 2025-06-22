using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.EntityFrameworkCore;
using ZapperHub.Data;
using ZapperHub.Hardware;
using ZapperHub.Services;

var builder = WebApplication.CreateBuilder(args);

// Add FastEndpoints
builder.Services.AddFastEndpoints();
builder.Services.SwaggerDocument();

// Add Entity Framework
builder.Services.AddDbContext<ZapperContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=zapper.db"));

// Add services
builder.Services.AddScoped<IDeviceService, DeviceService>();
builder.Services.AddScoped<IActivityService, ActivityService>();

// Add hardware services
builder.Services.AddSingleton<IInfraredTransmitter, MockInfraredTransmitter>();
builder.Services.AddSingleton<INetworkDeviceController, NetworkDeviceController>();
builder.Services.AddSingleton<IUsbRemoteHandler, MockUsbRemoteHandler>();
builder.Services.AddScoped<IWebOSClient, WebOSClient>();
builder.Services.AddScoped<WebOSDeviceController>();

// Add HttpClient for network operations
builder.Services.AddHttpClient();

// Add logging
builder.Services.AddLogging();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseFastEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
}

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ZapperContext>();
    context.Database.EnsureCreated();
}

app.Run();
