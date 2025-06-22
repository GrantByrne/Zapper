using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.EntityFrameworkCore;
using Zapper.Data;
using Zapper.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddDbContext<ZapperContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register services
builder.Services.AddScoped<IDeviceService, DeviceService>();
builder.Services.AddScoped<IActivityService, ActivityService>();
builder.Services.AddScoped<IIRCodeService, IRCodeService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// Add FastEndpoints
builder.Services.AddFastEndpoints();

// Add Swagger documentation
builder.Services.SwaggerDocument();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerGen();
}

app.UseHttpsRedirection();

// Use FastEndpoints
app.UseFastEndpoints();

app.Run();