namespace Zapper.Device.AndroidTV;

public interface IAdbClient : IDisposable
{
    Task<bool> ConnectAsync(string host, int port = 5555, CancellationToken cancellationToken = default);
    Task DisconnectAsync();
    Task<bool> ExecuteShellCommandAsync(string command, CancellationToken cancellationToken = default);
    Task<string?> ExecuteShellCommandWithResponseAsync(string command, CancellationToken cancellationToken = default);
    bool IsConnected { get; }
}