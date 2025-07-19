using Microsoft.Extensions.Logging;
using Zapper.Core.Models;
using Zapper.Data;
using Zapper.Device.AppleTV.Interfaces;
using Zapper.Device.AppleTV.Models;

namespace Zapper.Device.AppleTV.Services;

public class AppleTvService(
    ILogger<AppleTvService> logger,
    AppleTvDiscoveryService discoveryService,
    AppleTvControllerFactory controllerFactory,
    ZapperContext context)
{
    private readonly Dictionary<int, IAppleTvController> _activeControllers = new();

    public async Task<List<AppleTvDevice>> DiscoverAppleTvsAsync(int timeoutSeconds = 5)
    {
        logger.LogInformation("Starting Apple TV discovery");
        return await discoveryService.DiscoverDevicesAsync(timeoutSeconds);
    }

    public async Task<Zapper.Core.Models.Device?> CreateDeviceFromDiscoveredAsync(AppleTvDevice discoveredDevice)
    {
        try
        {
            var device = new Zapper.Core.Models.Device
            {
                Name = discoveredDevice.Name,
                Brand = "Apple",
                Model = discoveredDevice.Model,
                Type = DeviceType.StreamingDevice,
                ConnectionType = discoveredDevice.PreferredProtocol,
                IpAddress = discoveredDevice.IpAddress,
                Port = discoveredDevice.Port,
                DeviceIdentifier = discoveredDevice.Identifier,
                ProtocolVersion = discoveredDevice.Version,
                RequiresPairing = discoveredDevice.RequiresPairing,
                IsPaired = discoveredDevice.IsPaired,
                ServiceProperties = discoveredDevice.Properties,
                IsOnline = true,
                SupportsKeyboardInput = true,
                SupportsMouseInput = false
            };

            context.Devices.Add(device);
            await context.SaveChangesAsync();
            logger.LogInformation("Created Apple TV device: {Name}", device.Name);

            return device;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating device from discovered Apple TV");
            return null;
        }
    }

    public async Task<bool> ConnectToDeviceAsync(Zapper.Core.Models.Device device)
    {
        if (_activeControllers.ContainsKey(device.Id))
        {
            logger.LogWarning("Already connected to device {Id}", device.Id);
            return true;
        }

        try
        {
            var controller = controllerFactory.CreateControllerForDevice(device);
            var connected = await controller.ConnectAsync(device);

            if (connected)
            {
                _activeControllers[device.Id] = controller;
                device.IsOnline = true;
                await context.SaveChangesAsync();
                logger.LogInformation("Connected to Apple TV {Name}", device.Name);
            }

            return connected;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error connecting to Apple TV {Name}", device.Name);
            return false;
        }
    }

    public async Task<bool> DisconnectFromDeviceAsync(int deviceId)
    {
        if (!_activeControllers.TryGetValue(deviceId, out var controller))
        {
            logger.LogWarning("No active connection for device {Id}", deviceId);
            return true;
        }

        try
        {
            await controller.DisconnectAsync();
            _activeControllers.Remove(deviceId);

            var device = await context.Devices.FindAsync(deviceId);
            if (device != null)
            {
                device.IsOnline = false;
                await context.SaveChangesAsync();
            }

            logger.LogInformation("Disconnected from Apple TV");
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error disconnecting from Apple TV");
            return false;
        }
    }

    public async Task<bool> SendCommandAsync(int deviceId, DeviceCommand command)
    {
        if (!_activeControllers.TryGetValue(deviceId, out var controller))
        {
            logger.LogWarning("No active connection for device {Id}", deviceId);

            var device = await context.Devices.FindAsync(deviceId);
            if (device == null)
            {
                logger.LogError("Device {Id} not found", deviceId);
                return false;
            }

            if (!await ConnectToDeviceAsync(device))
            {
                logger.LogError("Failed to connect to device {Id}", deviceId);
                return false;
            }

            controller = _activeControllers[deviceId];
        }

        return await controller.SendCommandAsync(command);
    }

    public async Task<AppleTvStatus?> GetStatusAsync(int deviceId)
    {
        if (!_activeControllers.TryGetValue(deviceId, out var controller))
        {
            logger.LogWarning("No active connection for device {Id}", deviceId);
            return null;
        }

        return await controller.GetStatusAsync();
    }

    public async Task<bool> PairDeviceAsync(int deviceId, string pin)
    {
        var device = await context.Devices.FindAsync(deviceId);
        if (device == null)
        {
            logger.LogError("Device {Id} not found", deviceId);
            return false;
        }

        IAppleTvController controller;
        if (!_activeControllers.TryGetValue(deviceId, out controller!))
        {
            if (!await ConnectToDeviceAsync(device))
            {
                logger.LogError("Failed to connect to device for pairing");
                return false;
            }
            controller = _activeControllers[deviceId];
        }

        var paired = await controller.PairAsync(pin);
        if (paired)
        {
            device.IsPaired = true;
            await context.SaveChangesAsync();
            logger.LogInformation("Successfully paired with Apple TV {Name}", device.Name);
        }

        return paired;
    }

    public async Task DisconnectAllAsync()
    {
        var tasks = _activeControllers.Keys.Select(DisconnectFromDeviceAsync);
        await Task.WhenAll(tasks);
    }
}