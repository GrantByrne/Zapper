using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Zapper.Core.Models;
using Zapper.Device.AppleTV.Controllers;
using Zapper.Device.AppleTV.Interfaces;

namespace Zapper.Device.AppleTV.Services;

public class AppleTvControllerFactory(IServiceProvider serviceProvider, ILogger<AppleTvControllerFactory> logger)
{
    public virtual IAppleTvController CreateController(ConnectionType connectionType)
    {
        logger.LogInformation("Creating Apple TV controller for protocol: {Protocol}", connectionType);

        return connectionType switch
        {
            ConnectionType.CompanionProtocol => serviceProvider.GetRequiredService<CompanionProtocolController>(),
            ConnectionType.MediaRemoteProtocol => serviceProvider.GetRequiredService<MrpProtocolController>(),
            ConnectionType.Dacp => throw new NotImplementedException("DACP protocol not yet implemented"),
            ConnectionType.AirPlay => throw new NotImplementedException("AirPlay protocol not yet implemented"),
            _ => throw new NotSupportedException($"Connection type {connectionType} is not supported for Apple TV")
        };
    }

    public virtual IAppleTvController CreateControllerForDevice(Zapper.Core.Models.Device device)
    {
        var preferredProtocol = DetermineProtocolForDevice(device);
        return CreateController(preferredProtocol);
    }

    private ConnectionType DetermineProtocolForDevice(Zapper.Core.Models.Device device)
    {
        if (!string.IsNullOrEmpty(device.ProtocolVersion))
        {
            if (int.TryParse(device.ProtocolVersion.Split('.')[0], out var majorVersion))
            {
                if (majorVersion >= 15)
                    return ConnectionType.CompanionProtocol;
            }
        }

        if (device.ConnectionType == ConnectionType.CompanionProtocol ||
            device.ConnectionType == ConnectionType.MediaRemoteProtocol ||
            device.ConnectionType == ConnectionType.Dacp ||
            device.ConnectionType == ConnectionType.AirPlay)
        {
            return device.ConnectionType;
        }

        return ConnectionType.MediaRemoteProtocol;
    }
}