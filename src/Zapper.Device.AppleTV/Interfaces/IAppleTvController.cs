using Zapper.Core.Models;
using Zapper.Device.AppleTV.Models;

namespace Zapper.Device.AppleTV.Interfaces;

public interface IAppleTvController
{
    Task<bool> ConnectAsync(Zapper.Core.Models.Device device);
    Task<bool> DisconnectAsync();
    Task<bool> SendCommandAsync(DeviceCommand command);
    Task<AppleTvStatus?> GetStatusAsync();
    Task<bool> PairAsync(string pin);
    Task<bool> IsConnectedAsync();
    ConnectionType SupportedProtocol { get; }
}