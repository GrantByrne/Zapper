using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Zapper.Integrations;

public class WebOSClient : IWebOSClient, IDisposable
{
    private ClientWebSocket? _webSocket;
    private readonly ILogger<WebOSClient> _logger;
    private int _messageId = 1;
    private string? _clientKey;
    private bool _isAuthenticated;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<string?>> _pendingRequests = new();

    public string? ClientKey => _clientKey;
    public bool IsConnected => _webSocket?.State == WebSocketState.Open;

    public WebOSClient(ILogger<WebOSClient> logger)
    {
        _logger = logger;
    }

    public async Task<bool> ConnectAsync(string ipAddress, bool useSecure = false, CancellationToken cancellationToken = default)
    {
        try
        {
            _webSocket?.Dispose();
            _webSocket = new ClientWebSocket();

            var protocol = useSecure ? "wss" : "ws";
            var port = useSecure ? 3001 : 3000;
            var uri = new Uri($"{protocol}://{ipAddress}:{port}");

            _logger.LogInformation("Connecting to webOS TV at {Uri}", uri);

            await _webSocket.ConnectAsync(uri, cancellationToken);

            // Start listening for responses
            _ = Task.Run(() => ListenForMessagesAsync(cancellationToken), cancellationToken);

            return IsConnected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to webOS TV at {IpAddress}", ipAddress);
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
        _isAuthenticated = false;
    }

    public async Task<bool> AuthenticateAsync(string? storedKey = null, CancellationToken cancellationToken = default)
    {
        if (!IsConnected)
        {
            _logger.LogWarning("Cannot authenticate - not connected to TV");
            return false;
        }

        try
        {
            var authPayload = new
            {
                pairingType = "PROMPT",
                client_key = storedKey,
                manifest = new
                {
                    manifestVersion = 1,
                    appId = "com.zapperhub.remote",
                    vendorId = "com.zapperhub",
                    localizedAppNames = new
                    {
                        en = "ZapperHub Remote"
                    },
                    localizedVendorNames = new
                    {
                        en = "ZapperHub"
                    },
                    permissions = new[]
                    {
                        "LAUNCH",
                        "LAUNCH_WEBAPP",
                        "APP_TO_APP",
                        "CLOSE",
                        "TEST_OPEN",
                        "TEST_PROTECTED",
                        "CONTROL_AUDIO",
                        "CONTROL_DISPLAY",
                        "CONTROL_INPUT_JOYSTICK",
                        "CONTROL_INPUT_MEDIA_RECORDING",
                        "CONTROL_INPUT_MEDIA_PLAYBACK",
                        "CONTROL_INPUT_TV",
                        "CONTROL_POWER",
                        "READ_APP_STATUS",
                        "READ_CURRENT_CHANNEL",
                        "READ_INPUT_DEVICE_LIST",
                        "READ_NETWORK_STATE",
                        "READ_RUNNING_APPS",
                        "READ_TV_CHANNEL_LIST",
                        "WRITE_NOTIFICATION_TOAST",
                        "READ_POWER_STATE",
                        "READ_COUNTRY_INFO"
                    },
                    signatures = new object[0]
                }
            };

            var response = await SendCommandAsync("ssap://com.webos.service.pairing/register", authPayload, cancellationToken);
            
            if (response != null)
            {
                var responseObj = JsonSerializer.Deserialize<JsonElement>(response);
                if (responseObj.TryGetProperty("payload", out var payload) && 
                    payload.TryGetProperty("client-key", out var clientKeyElement))
                {
                    _clientKey = clientKeyElement.GetString();
                    _isAuthenticated = true;
                    _logger.LogInformation("Successfully authenticated with webOS TV");
                    return true;
                }
            }

            _logger.LogWarning("Authentication failed - invalid response");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Authentication failed");
            return false;
        }
    }

    public async Task<string?> SendCommandAsync(string uri, object? payload = null, CancellationToken cancellationToken = default)
    {
        if (!IsConnected)
        {
            _logger.LogWarning("Cannot send command - not connected to TV");
            return null;
        }

        try
        {
            var messageId = _messageId++;
            var message = new
            {
                type = "request",
                id = messageId.ToString(),
                uri = uri,
                payload = payload ?? new object()
            };

            var json = JsonSerializer.Serialize(message);
            var bytes = Encoding.UTF8.GetBytes(json);

            var tcs = new TaskCompletionSource<string?>();
            _pendingRequests[messageId.ToString()] = tcs;

            await _webSocket!.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, cancellationToken);

            // Wait for response with timeout
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(10));

            try
            {
                return await tcs.Task.WaitAsync(timeoutCts.Token);
            }
            catch (OperationCanceledException)
            {
                _pendingRequests.TryRemove(messageId.ToString(), out _);
                _logger.LogWarning("Command {Uri} timed out", uri);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send command {Uri}", uri);
            return null;
        }
    }

    private async Task ListenForMessagesAsync(CancellationToken cancellationToken)
    {
        var buffer = new byte[4096];

        try
        {
            while (_webSocket?.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
            {
                var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    ProcessMessage(message);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation is requested
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in message listener");
        }
    }

    private void ProcessMessage(string message)
    {
        try
        {
            var messageObj = JsonSerializer.Deserialize<JsonElement>(message);
            
            if (messageObj.TryGetProperty("id", out var idElement))
            {
                var id = idElement.GetString();
                if (id != null && _pendingRequests.TryRemove(id, out var tcs))
                {
                    tcs.SetResult(message);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process message: {Message}", message);
        }
    }

    public async Task<bool> PowerOffAsync(CancellationToken cancellationToken = default)
    {
        var response = await SendCommandAsync("ssap://system/turnOff", null, cancellationToken);
        return response != null;
    }

    public async Task<bool> SetVolumeAsync(int volume, CancellationToken cancellationToken = default)
    {
        var payload = new { volume = Math.Clamp(volume, 0, 100) };
        var response = await SendCommandAsync("ssap://audio/setVolume", payload, cancellationToken);
        return response != null;
    }

    public async Task<bool> VolumeUpAsync(CancellationToken cancellationToken = default)
    {
        var response = await SendCommandAsync("ssap://audio/volumeUp", null, cancellationToken);
        return response != null;
    }

    public async Task<bool> VolumeDownAsync(CancellationToken cancellationToken = default)
    {
        var response = await SendCommandAsync("ssap://audio/volumeDown", null, cancellationToken);
        return response != null;
    }

    public async Task<bool> SetMuteAsync(bool muted, CancellationToken cancellationToken = default)
    {
        var payload = new { mute = muted };
        var response = await SendCommandAsync("ssap://audio/setMute", payload, cancellationToken);
        return response != null;
    }

    public async Task<bool> LaunchAppAsync(string appId, CancellationToken cancellationToken = default)
    {
        var payload = new { id = appId };
        var response = await SendCommandAsync("ssap://com.webos.applicationManager/launch", payload, cancellationToken);
        return response != null;
    }

    public async Task<bool> SwitchInputAsync(string inputId, CancellationToken cancellationToken = default)
    {
        var payload = new { inputId = inputId };
        var response = await SendCommandAsync("ssap://tv/switchInput", payload, cancellationToken);
        return response != null;
    }

    public async Task<bool> ChannelUpAsync(CancellationToken cancellationToken = default)
    {
        var response = await SendCommandAsync("ssap://tv/channelUp", null, cancellationToken);
        return response != null;
    }

    public async Task<bool> ChannelDownAsync(CancellationToken cancellationToken = default)
    {
        var response = await SendCommandAsync("ssap://tv/channelDown", null, cancellationToken);
        return response != null;
    }

    public async Task<bool> ShowToastAsync(string message, CancellationToken cancellationToken = default)
    {
        var payload = new { message = message };
        var response = await SendCommandAsync("ssap://system.notifications/createToast", payload, cancellationToken);
        return response != null;
    }

    public void Dispose()
    {
        _webSocket?.Dispose();
    }
}