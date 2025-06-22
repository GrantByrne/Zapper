using System.Net.WebSockets;

namespace Zapper.Hardware;

public interface IWebOSClient
{
    Task<bool> ConnectAsync(string ipAddress, bool useSecure = false, CancellationToken cancellationToken = default);
    Task DisconnectAsync(CancellationToken cancellationToken = default);
    Task<bool> AuthenticateAsync(string? storedKey = null, CancellationToken cancellationToken = default);
    Task<string?> SendCommandAsync(string uri, object? payload = null, CancellationToken cancellationToken = default);
    Task<bool> PowerOffAsync(CancellationToken cancellationToken = default);
    Task<bool> SetVolumeAsync(int volume, CancellationToken cancellationToken = default);
    Task<bool> VolumeUpAsync(CancellationToken cancellationToken = default);
    Task<bool> VolumeDownAsync(CancellationToken cancellationToken = default);
    Task<bool> SetMuteAsync(bool muted, CancellationToken cancellationToken = default);
    Task<bool> LaunchAppAsync(string appId, CancellationToken cancellationToken = default);
    Task<bool> SwitchInputAsync(string inputId, CancellationToken cancellationToken = default);
    Task<bool> ChannelUpAsync(CancellationToken cancellationToken = default);
    Task<bool> ChannelDownAsync(CancellationToken cancellationToken = default);
    Task<bool> ShowToastAsync(string message, CancellationToken cancellationToken = default);
    string? ClientKey { get; }
    bool IsConnected { get; }
}