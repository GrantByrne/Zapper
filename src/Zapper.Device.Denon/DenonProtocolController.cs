using System.Net.Http;
using Microsoft.Extensions.Logging;
using Zapper.Core.Interfaces;
using Zapper.Core.Models;
using ZapperDevice = Zapper.Core.Models.Device;

namespace Zapper.Device.Denon;

public class DenonProtocolController(
    IHttpClientFactory httpClientFactory,
    ILoggerFactory loggerFactory,
    ILogger<DenonProtocolController> logger) : IDeviceController
{
    public async Task<bool> SendCommand(ZapperDevice device, DeviceCommand command)
    {
        try
        {
            if (device.ConnectionType != ConnectionType.Network || device.Type != DeviceType.DenonReceiver)
            {
                logger.LogWarning("Device {DeviceName} is not a Denon receiver", device.Name);
                return false;
            }

            var httpClient = CreateHttpClient(device);
            var denonController = new DenonDeviceController(httpClient, loggerFactory.CreateLogger<DenonDeviceController>());

            return command.Name.ToUpperInvariant() switch
            {
                "POWER_ON" => await denonController.SetPowerAsync(true),
                "POWER_OFF" => await denonController.SetPowerAsync(false),
                "POWER_TOGGLE" => await TogglePowerAsync(denonController),
                "VOLUME_UP" => await denonController.VolumeUpAsync(),
                "VOLUME_DOWN" => await denonController.VolumeDownAsync(),
                "MUTE_ON" => await denonController.SetMuteAsync(true),
                "MUTE_OFF" => await denonController.SetMuteAsync(false),
                "MUTE_TOGGLE" => await ToggleMuteAsync(denonController),
                _ when command.Name.StartsWith("INPUT_", StringComparison.OrdinalIgnoreCase)
                    => await denonController.SetInputAsync(command.Name[6..]),
                _ when command.Name.StartsWith("VOLUME_", StringComparison.OrdinalIgnoreCase) && int.TryParse(command.Name[7..], out var volume)
                    => await denonController.SetVolumeAsync(volume),
                _ when command.Name.StartsWith("ZONE_", StringComparison.OrdinalIgnoreCase)
                    => await HandleZoneCommand(denonController, command.Name),
                _ => false
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending command {CommandName} to device {DeviceName}",
                command.Name, device.Name);
            return false;
        }
    }

    public async Task<bool> TestConnection(ZapperDevice device)
    {
        try
        {
            if (!SupportsDevice(device))
                return false;

            var httpClient = CreateHttpClient(device);
            var denonController = new DenonDeviceController(httpClient, loggerFactory.CreateLogger<DenonDeviceController>());

            var modelInfo = await denonController.GetModelInfoAsync();
            return !string.IsNullOrEmpty(modelInfo) && modelInfo != "Denon AVR";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error testing connection to device {DeviceName}", device.Name);
            return false;
        }
    }

    public async Task<DeviceStatus> GetStatus(ZapperDevice device)
    {
        try
        {
            if (!SupportsDevice(device))
            {
                return new DeviceStatus
                {
                    IsOnline = false,
                    StatusMessage = "Device not supported"
                };
            }

            var httpClient = CreateHttpClient(device);
            var denonController = new DenonDeviceController(httpClient, loggerFactory.CreateLogger<DenonDeviceController>());

            var powerTask = denonController.GetPowerStatusAsync();
            var volumeTask = denonController.GetVolumeAsync();
            var muteTask = denonController.GetMuteStatusAsync();
            var inputTask = denonController.GetCurrentInputAsync();
            var modelTask = denonController.GetModelInfoAsync();

            await Task.WhenAll(powerTask, volumeTask, muteTask, inputTask, modelTask);

            return new DeviceStatus
            {
                IsOnline = true,
                StatusMessage = "Connected",
                Properties = new Dictionary<string, object>
                {
                    ["Power"] = powerTask.Result ? "On" : "Off",
                    ["Volume"] = volumeTask.Result,
                    ["Mute"] = muteTask.Result ? "On" : "Off",
                    ["Input"] = inputTask.Result,
                    ["Model"] = modelTask.Result
                }
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting status for device {DeviceName}", device.Name);
            return new DeviceStatus
            {
                IsOnline = false,
                StatusMessage = ex.Message
            };
        }
    }

    public bool SupportsDevice(ZapperDevice device)
    {
        return device.ConnectionType == ConnectionType.Network &&
               device.Type == DeviceType.DenonReceiver;
    }

    private HttpClient CreateHttpClient(ZapperDevice device)
    {
        var httpClient = httpClientFactory.CreateClient();
        httpClient.BaseAddress = new Uri($"http://{device.IpAddress ?? device.NetworkAddress}:{device.Port ?? 80}");
        httpClient.Timeout = TimeSpan.FromSeconds(10);
        return httpClient;
    }

    private async Task<bool> TogglePowerAsync(DenonDeviceController controller)
    {
        var currentPower = await controller.GetPowerStatusAsync();
        return await controller.SetPowerAsync(!currentPower);
    }

    private async Task<bool> ToggleMuteAsync(DenonDeviceController controller)
    {
        var currentMute = await controller.GetMuteStatusAsync();
        return await controller.SetMuteAsync(!currentMute);
    }

    private async Task<bool> HandleZoneCommand(DenonDeviceController controller, string commandName)
    {
        var parts = commandName.Split('_');
        if (parts.Length < 3)
            return false;

        var zone = parts[1];
        var action = string.Join("_", parts.Skip(2));

        return action switch
        {
            "POWER_ON" => await controller.SetZonePowerAsync(zone, true),
            "POWER_OFF" => await controller.SetZonePowerAsync(zone, false),
            _ when action.StartsWith("VOLUME_") && int.TryParse(action[7..], out var volume)
                => await controller.SetZoneVolumeAsync(zone, volume),
            _ when action.StartsWith("INPUT_")
                => await controller.SetZoneInputAsync(zone, action[6..]),
            _ => false
        };
    }
}