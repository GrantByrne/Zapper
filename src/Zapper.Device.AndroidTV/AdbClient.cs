using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;
using Zapper.Device.AndroidTV.Models;

namespace Zapper.Device.AndroidTV;

public class AdbClient(ILogger<AdbClient> logger) : IAdbClient
{
    private readonly ILogger<AdbClient> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private TcpClient? _tcpClient;
    private NetworkStream? _stream;
    private uint _nextLocalId = 1;
    private readonly Dictionary<uint, TaskCompletionSource<bool>> _pendingCommands = new();
    private readonly object _lock = new();
    private bool _isConnected;

    public bool IsConnected => _isConnected && _tcpClient?.Connected == true;

    public async Task<bool> ConnectAsync(string host, int port = 5555, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Connecting to ADB at {Host}:{Port}", host, port);

            _tcpClient = new TcpClient();
            await _tcpClient.ConnectAsync(host, port, cancellationToken);
            _stream = _tcpClient.GetStream();

            var connectMessage = new AdbMessage
            {
                Command = AdbCommands.Connect,
                Arg0 = AdbConstants.Version,
                Arg1 = AdbConstants.MaxPayload,
                DataLength = (uint)AdbConstants.SystemIdentity.Length,
                Data = Encoding.UTF8.GetBytes(AdbConstants.SystemIdentity),
                Magic = AdbCommands.Connect ^ 0xffffffff
            };

            await SendMessageAsync(connectMessage, cancellationToken);

            var response = await ReceiveMessageAsync(cancellationToken);

            if (response?.Command == AdbCommands.Connect)
            {
                _logger.LogInformation("ADB connection established");
                _isConnected = true;
                return true;
            }

            if (response?.Command == AdbCommands.Auth)
            {
                _logger.LogWarning("ADB requires authentication - device may not be authorized");
                return false;
            }

            _logger.LogError("Unexpected ADB response: {Command:X8}", response?.Command);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to ADB at {Host}:{Port}", host, port);
            await DisconnectAsync();
            return false;
        }
    }

    public async Task DisconnectAsync()
    {
        _isConnected = false;

        if (_stream != null)
        {
            await _stream.DisposeAsync();
            _stream = null;
        }

        _tcpClient?.Close();
        _tcpClient?.Dispose();
        _tcpClient = null;

        lock (_lock)
        {
            foreach (var pending in _pendingCommands.Values)
            {
                pending.TrySetResult(false);
            }
            _pendingCommands.Clear();
        }
    }

    public async Task<bool> ExecuteShellCommandAsync(string command, CancellationToken cancellationToken = default)
    {
        if (!IsConnected)
        {
            _logger.LogWarning("Cannot execute command - not connected to ADB");
            return false;
        }

        try
        {
            var shellService = "shell:" + command + "\0";
            var openMessage = new AdbMessage
            {
                Command = AdbCommands.Open,
                Arg0 = GetNextLocalId(),
                Arg1 = 0,
                DataLength = (uint)shellService.Length,
                Data = Encoding.UTF8.GetBytes(shellService),
                Magic = AdbCommands.Open ^ 0xffffffff
            };

            await SendMessageAsync(openMessage, cancellationToken);

            var response = await ReceiveMessageAsync(cancellationToken);

            if (response?.Command == AdbCommands.Okay)
            {
                var closeMessage = new AdbMessage
                {
                    Command = AdbCommands.Close,
                    Arg0 = openMessage.Arg0,
                    Arg1 = response.Arg0,
                    DataLength = 0,
                    Magic = AdbCommands.Close ^ 0xffffffff
                };

                await SendMessageAsync(closeMessage, cancellationToken);
                return true;
            }

            _logger.LogError("Shell command failed - unexpected response: {Command:X8}", response?.Command);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute shell command: {Command}", command);
            return false;
        }
    }

    public async Task<string?> ExecuteShellCommandWithResponseAsync(string command, CancellationToken cancellationToken = default)
    {
        if (!IsConnected)
        {
            _logger.LogWarning("Cannot execute command - not connected to ADB");
            return null;
        }

        try
        {
            var shellService = "shell:" + command + "\0";
            var openMessage = new AdbMessage
            {
                Command = AdbCommands.Open,
                Arg0 = GetNextLocalId(),
                Arg1 = 0,
                DataLength = (uint)shellService.Length,
                Data = Encoding.UTF8.GetBytes(shellService),
                Magic = AdbCommands.Open ^ 0xffffffff
            };

            await SendMessageAsync(openMessage, cancellationToken);

            var response = await ReceiveMessageAsync(cancellationToken);

            if (response?.Command == AdbCommands.Okay)
            {
                var dataResponse = await ReceiveMessageAsync(cancellationToken);
                if (dataResponse?.Command == AdbCommands.Write && dataResponse.Data.Length > 0)
                {
                    var result = Encoding.UTF8.GetString(dataResponse.Data);

                    var closeMessage = new AdbMessage
                    {
                        Command = AdbCommands.Close,
                        Arg0 = openMessage.Arg0,
                        Arg1 = response.Arg0,
                        DataLength = 0,
                        Magic = AdbCommands.Close ^ 0xffffffff
                    };

                    await SendMessageAsync(closeMessage, cancellationToken);
                    return result.Trim();
                }
            }

            _logger.LogError("Shell command with response failed - unexpected response: {Command:X8}", response?.Command);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute shell command with response: {Command}", command);
            return null;
        }
    }

    private async Task SendMessageAsync(AdbMessage message, CancellationToken cancellationToken)
    {
        if (_stream == null)
            throw new InvalidOperationException("No active ADB connection");

        var bytes = message.ToBytes();
        await _stream.WriteAsync(bytes, cancellationToken);
        await _stream.FlushAsync(cancellationToken);
    }

    private async Task<AdbMessage?> ReceiveMessageAsync(CancellationToken cancellationToken)
    {
        if (_stream == null)
            throw new InvalidOperationException("No active ADB connection");

        var headerBuffer = new byte[AdbMessage.AdbHeaderLength];
        var bytesRead = 0;

        while (bytesRead < AdbMessage.AdbHeaderLength)
        {
            var read = await _stream.ReadAsync(headerBuffer.AsMemory(bytesRead), cancellationToken);
            if (read == 0)
                return null;
            bytesRead += read;
        }

        var message = AdbMessage.FromBytes(headerBuffer);

        if (message.DataLength > 0)
        {
            var dataBuffer = new byte[message.DataLength];
            bytesRead = 0;

            while (bytesRead < message.DataLength)
            {
                var read = await _stream.ReadAsync(dataBuffer.AsMemory(bytesRead), cancellationToken);
                if (read == 0)
                    break;
                bytesRead += read;
            }

            message.Data = dataBuffer;
        }

        return message;
    }

    private uint GetNextLocalId()
    {
        return Interlocked.Increment(ref _nextLocalId);
    }

    public void Dispose()
    {
        DisconnectAsync().GetAwaiter().GetResult();
        GC.SuppressFinalize(this);
    }
}