namespace Zapper.Device.Tizen;

public interface ITizenClient
{
    Task<bool> ConnectAsync(string ipAddress, string? token = null, CancellationToken cancellationToken = default);
    Task DisconnectAsync(CancellationToken cancellationToken = default);
    Task<string?> AuthenticateAsync(string appName = "Zapper", CancellationToken cancellationToken = default);
    Task<bool> SendCommandAsync(string method, object? parameters = null, CancellationToken cancellationToken = default);
    Task<bool> SendKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<bool> PowerOffAsync(CancellationToken cancellationToken = default);
    Task<bool> SetVolumeAsync(int volume, CancellationToken cancellationToken = default);
    Task<bool> VolumeUpAsync(CancellationToken cancellationToken = default);
    Task<bool> VolumeDownAsync(CancellationToken cancellationToken = default);
    Task<bool> SetMuteAsync(bool muted, CancellationToken cancellationToken = default);
    Task<bool> LaunchAppAsync(string appId, CancellationToken cancellationToken = default);
    Task<bool> SwitchInputAsync(string inputId, CancellationToken cancellationToken = default);
    Task<bool> ChannelUpAsync(CancellationToken cancellationToken = default);
    Task<bool> ChannelDownAsync(CancellationToken cancellationToken = default);
    Task<bool> SendTextAsync(string text, CancellationToken cancellationToken = default);
    string? AuthToken { get; }
    bool IsConnected { get; }
}