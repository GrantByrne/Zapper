using Microsoft.Extensions.Logging;
using Zapper.Core.Models;
using Zapper.Device.AppleTV.Interfaces;
using Zapper.Device.AppleTV.Models;

namespace Zapper.Device.AppleTV.Controllers;

public abstract class BaseAppleTvController : IAppleTvController
{
    protected readonly ILogger<BaseAppleTvController> Logger;
    protected Zapper.Core.Models.Device? ConnectedDevice;
    protected bool IsConnected;

    protected BaseAppleTvController(ILogger<BaseAppleTvController> logger)
    {
        Logger = logger;
    }

    public abstract ConnectionType SupportedProtocol { get; }

    public virtual async Task<bool> ConnectAsync(Zapper.Core.Models.Device device)
    {
        try
        {
            Logger.LogInformation("Connecting to Apple TV {Name} at {Address} using {Protocol}",
                device.Name, device.IpAddress, SupportedProtocol);

            ConnectedDevice = device;
            var connected = await EstablishConnectionAsync(device);
            IsConnected = connected;

            return connected;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error connecting to Apple TV");
            IsConnected = false;
            return false;
        }
    }

    public virtual async Task<bool> DisconnectAsync()
    {
        try
        {
            Logger.LogInformation("Disconnecting from Apple TV");
            await CloseConnectionAsync();
            IsConnected = false;
            ConnectedDevice = null;
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error disconnecting from Apple TV");
            return false;
        }
    }

    public abstract Task<bool> SendCommandAsync(DeviceCommand command);

    public abstract Task<AppleTvStatus?> GetStatusAsync();

    public abstract Task<bool> PairAsync(string pin);

    public virtual Task<bool> IsConnectedAsync()
    {
        return Task.FromResult(IsConnected);
    }

    protected abstract Task<bool> EstablishConnectionAsync(Zapper.Core.Models.Device device);

    protected abstract Task CloseConnectionAsync();

    protected virtual CommandCode MapDeviceCommand(DeviceCommand command)
    {
        return command.Name.ToLowerInvariant() switch
        {
            "poweron" or "power" => CommandCode.Wake,
            "poweroff" => CommandCode.Sleep,
            "volumeup" => CommandCode.VolumeUp,
            "volumedown" => CommandCode.VolumeDown,
            "mute" => CommandCode.Mute,
            "menu" => CommandCode.Menu,
            "home" => CommandCode.Home,
            "back" => CommandCode.Back,
            "select" or "ok" => CommandCode.Select,
            "up" or "directionalup" => CommandCode.Up,
            "down" or "directionaldown" => CommandCode.Down,
            "left" or "directionalleft" => CommandCode.Left,
            "right" or "directionalright" => CommandCode.Right,
            "play" => CommandCode.Play,
            "pause" => CommandCode.Pause,
            "stop" => CommandCode.Stop,
            "fastforward" or "forward" => CommandCode.FastForward,
            "rewind" => CommandCode.Rewind,
            "nexttrack" or "next" => CommandCode.Next,
            "previoustrack" or "previous" => CommandCode.Previous,
            _ => CommandCode.Unknown
        };
    }
}