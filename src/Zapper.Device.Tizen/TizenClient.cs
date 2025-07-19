using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Zapper.Device.Tizen;

public class TizenClient(ILogger<TizenClient> logger) : ITizenClient, IDisposable
{
    private ClientWebSocket? _webSocket;
    private string? _authToken;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<bool>> _pendingRequests = new();
    private const int WebSocketPort = 8002;
    private const int LegacyPort = 8001;
    private const string TokenPath = "/api/v2/channels/samsung.remote.control";

    public string? AuthToken => _authToken;
    public bool IsConnected => _webSocket?.State == WebSocketState.Open;

    public async Task<bool> ConnectAsync(string ipAddress, string? token = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _webSocket?.Dispose();
            _webSocket = new ClientWebSocket();
            _webSocket.Options.KeepAliveInterval = TimeSpan.FromSeconds(30);

            _authToken = token;

            var uri = BuildWebSocketUri(ipAddress, token);
            logger.LogInformation("Connecting to Samsung Tizen TV at {Uri}", uri);

            await _webSocket.ConnectAsync(uri, cancellationToken);

            _ = Task.Run(() => ListenForMessagesAsync(cancellationToken), cancellationToken);

            return IsConnected;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to connect to Samsung Tizen TV at {IpAddress}", ipAddress);
            return false;
        }
    }

    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        if (_webSocket?.State == WebSocketState.Open)
        {
            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disconnecting", cancellationToken);
        }
        _webSocket?.Dispose();
        _webSocket = null;
    }

    public Task<string?> AuthenticateAsync(string appName = "Zapper", CancellationToken cancellationToken = default)
    {
        if (!IsConnected)
        {
            logger.LogWarning("Cannot authenticate - not connected to TV");
            return Task.FromResult<string?>(null);
        }

        try
        {
            var token = GenerateAuthToken();
            _authToken = token;

            return Task.FromResult<string?>(token);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to authenticate with TV");
            return Task.FromResult<string?>(null);
        }
    }

    public async Task<bool> SendCommandAsync(string method, object? parameters = null, CancellationToken cancellationToken = default)
    {
        if (!IsConnected)
        {
            logger.LogWarning("Cannot send command - not connected to TV");
            return false;
        }

        try
        {
            var message = new
            {
                method,
                @params = parameters ?? new { }
            };

            var json = JsonSerializer.Serialize(message);
            var bytes = Encoding.UTF8.GetBytes(json);

            await _webSocket!.SendAsync(
                new ArraySegment<byte>(bytes),
                WebSocketMessageType.Text,
                true,
                cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send command {Method}", method);
            return false;
        }
    }

    public async Task<bool> SendKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        var keyCommand = new
        {
            Cmd = "Click",
            DataOfCmd = key,
            Option = "false",
            TypeOfRemote = "SendRemoteKey"
        };

        return await SendCommandAsync("ms.remote.control", keyCommand, cancellationToken);
    }

    public async Task<bool> PowerOffAsync(CancellationToken cancellationToken = default)
    {
        return await SendKeyAsync("KEY_POWER", cancellationToken);
    }

    public Task<bool> SetVolumeAsync(int volume, CancellationToken cancellationToken = default)
    {
        logger.LogWarning("Direct volume setting not supported on Tizen. Use VolumeUp/VolumeDown instead.");
        return Task.FromResult(false);
    }

    public async Task<bool> VolumeUpAsync(CancellationToken cancellationToken = default)
    {
        return await SendKeyAsync("KEY_VOLUP", cancellationToken);
    }

    public async Task<bool> VolumeDownAsync(CancellationToken cancellationToken = default)
    {
        return await SendKeyAsync("KEY_VOLDOWN", cancellationToken);
    }

    public async Task<bool> SetMuteAsync(bool muted, CancellationToken cancellationToken = default)
    {
        return await SendKeyAsync("KEY_MUTE", cancellationToken);
    }

    public async Task<bool> LaunchAppAsync(string appId, CancellationToken cancellationToken = default)
    {
        var appCommand = new
        {
            @event = "ed.apps.launch",
            to = "host",
            data = new
            {
                appId,
                actionType = "NATIVE_LAUNCH"
            }
        };

        return await SendCommandAsync("ms.channel.emit", appCommand, cancellationToken);
    }

    public async Task<bool> SwitchInputAsync(string inputId, CancellationToken cancellationToken = default)
    {
        return await SendKeyAsync($"KEY_{inputId}", cancellationToken);
    }

    public async Task<bool> ChannelUpAsync(CancellationToken cancellationToken = default)
    {
        return await SendKeyAsync("KEY_CHUP", cancellationToken);
    }

    public async Task<bool> ChannelDownAsync(CancellationToken cancellationToken = default)
    {
        return await SendKeyAsync("KEY_CHDOWN", cancellationToken);
    }

    public async Task<bool> SendTextAsync(string text, CancellationToken cancellationToken = default)
    {
        var textCommand = new
        {
            Cmd = Base64Encode(text),
            DataOfCmd = "base64",
            TypeOfRemote = "SendInputString"
        };

        return await SendCommandAsync("ms.remote.control", textCommand, cancellationToken);
    }

    private Uri BuildWebSocketUri(string ipAddress, string? token)
    {
        var appName = Base64Encode("Zapper");
        var tokenParam = string.IsNullOrEmpty(token) ? "" : $"&token={token}";

        return new Uri($"wss://{ipAddress}:{WebSocketPort}{TokenPath}?name={appName}{tokenParam}");
    }

    private string GenerateAuthToken()
    {
        var bytes = new byte[16];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    private static string Base64Encode(string plainText)
    {
        var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        return Convert.ToBase64String(plainTextBytes);
    }

    private async Task ListenForMessagesAsync(CancellationToken cancellationToken)
    {
        var buffer = new ArraySegment<byte>(new byte[4096]);
        var messageBuilder = new StringBuilder();

        try
        {
            while (_webSocket?.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
            {
                var result = await _webSocket.ReceiveAsync(buffer, cancellationToken);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    messageBuilder.Append(Encoding.UTF8.GetString(buffer.Array!, 0, result.Count));

                    if (result.EndOfMessage)
                    {
                        var message = messageBuilder.ToString();
                        messageBuilder.Clear();

                        logger.LogDebug("Received message from TV: {Message}", message);
                        ProcessMessage(message);
                    }
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    logger.LogInformation("WebSocket connection closed by TV");
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in message listener");
        }
    }

    private void ProcessMessage(string message)
    {
        try
        {
            var json = JsonDocument.Parse(message);

            if (json.RootElement.TryGetProperty("event", out var eventProp))
            {
                var eventName = eventProp.GetString();
                logger.LogDebug("Received event: {Event}", eventName);

                if (eventName == "ms.channel.connect")
                {
                    if (json.RootElement.TryGetProperty("data", out var data) &&
                        data.TryGetProperty("token", out var tokenProp))
                    {
                        _authToken = tokenProp.GetString();
                        logger.LogInformation("Received authentication token from TV");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process message: {Message}", message);
        }
    }

    public void Dispose()
    {
        _webSocket?.Dispose();
    }
}