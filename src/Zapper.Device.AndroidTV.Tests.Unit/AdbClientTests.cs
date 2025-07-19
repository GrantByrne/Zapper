using System.Net;
using System.Net.Sockets;
using System.Text;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Zapper.Device.AndroidTV.Models;

namespace Zapper.Device.AndroidTV.Tests.Unit;

public class AdbClientTests : IDisposable
{
    private readonly ILogger<AdbClient> _mockLogger;
    private readonly AdbClient _adbClient;
    private TcpListener? _tcpListener;
    private CancellationTokenSource _cancellationTokenSource;

    public AdbClientTests()
    {
        _mockLogger = Substitute.For<ILogger<AdbClient>>();
        _adbClient = new AdbClient(_mockLogger);
        _cancellationTokenSource = new CancellationTokenSource();
    }

    [Fact]
    public async Task ConnectAsync_ValidHost_ReturnsTrue()
    {
        // Arrange
        var port = GetAvailablePort();
        _tcpListener = new TcpListener(IPAddress.Loopback, port);
        _tcpListener.Start();

        var serverTask = Task.Run(async () =>
        {
            using var client = await _tcpListener.AcceptTcpClientAsync();
            using var stream = client.GetStream();
            
            // Read connect message
            var buffer = new byte[1024];
            _ = await stream.ReadAsync(buffer);
            
            // Send connect response
            var response = new AdbMessage
            {
                Command = AdbCommands.Connect,
                Arg0 = AdbConstants.Version,
                Arg1 = AdbConstants.MaxPayload,
                DataLength = 0,
                Magic = AdbCommands.Connect ^ 0xffffffff
            };
            
            await stream.WriteAsync(response.ToBytes());
        });

        // Act
        var result = await _adbClient.ConnectAsync("localhost", port, _cancellationTokenSource.Token);

        // Assert
        result.Should().BeTrue();
        _adbClient.IsConnected.Should().BeTrue();

        await serverTask;
    }

    [Fact]
    public async Task ConnectAsync_AuthRequired_ReturnsFalse()
    {
        // Arrange
        var port = GetAvailablePort();
        _tcpListener = new TcpListener(IPAddress.Loopback, port);
        _tcpListener.Start();

        var serverTask = Task.Run(async () =>
        {
            using var client = await _tcpListener.AcceptTcpClientAsync();
            using var stream = client.GetStream();
            
            // Read connect message
            var buffer = new byte[1024];
            _ = await stream.ReadAsync(buffer);
            
            // Send auth response
            var response = new AdbMessage
            {
                Command = AdbCommands.Auth,
                Arg0 = 1, // AUTH_TOKEN
                Arg1 = 0,
                DataLength = 0,
                Magic = AdbCommands.Auth ^ 0xffffffff
            };
            
            await stream.WriteAsync(response.ToBytes());
        });

        // Act
        var result = await _adbClient.ConnectAsync("localhost", port, _cancellationTokenSource.Token);

        // Assert
        result.Should().BeFalse();
        _adbClient.IsConnected.Should().BeFalse();

        await serverTask;
    }

    [Fact]
    public async Task ConnectAsync_InvalidHost_ReturnsFalse()
    {
        // Act
        var result = await _adbClient.ConnectAsync("invalid.host.name", 5555, _cancellationTokenSource.Token);

        // Assert
        result.Should().BeFalse();
        _adbClient.IsConnected.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteShellCommandAsync_NotConnected_ReturnsFalse()
    {
        // Act
        var result = await _adbClient.ExecuteShellCommandAsync("echo test");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteShellCommandAsync_Connected_SendsCommandAndReturnsTrue()
    {
        // Arrange
        var port = GetAvailablePort();
        _tcpListener = new TcpListener(IPAddress.Loopback, port);
        _tcpListener.Start();

        var serverTask = Task.Run(async () =>
        {
            using var client = await _tcpListener.AcceptTcpClientAsync();
            using var stream = client.GetStream();
            
            // Handle connect
            var buffer = new byte[1024];
            _ = await stream.ReadAsync(buffer);
            
            var connectResponse = new AdbMessage
            {
                Command = AdbCommands.Connect,
                Arg0 = AdbConstants.Version,
                Arg1 = AdbConstants.MaxPayload,
                DataLength = 0,
                Magic = AdbCommands.Connect ^ 0xffffffff
            };
            await stream.WriteAsync(connectResponse.ToBytes());
            
            // Handle shell command
            _ = await stream.ReadAsync(buffer);
            
            var okayResponse = new AdbMessage
            {
                Command = AdbCommands.Okay,
                Arg0 = 1,
                Arg1 = 1,
                DataLength = 0,
                Magic = AdbCommands.Okay ^ 0xffffffff
            };
            await stream.WriteAsync(okayResponse.ToBytes());
            
            // Handle close
            _ = await stream.ReadAsync(buffer);
        });

        await _adbClient.ConnectAsync("localhost", port, _cancellationTokenSource.Token);

        // Act
        var result = await _adbClient.ExecuteShellCommandAsync("echo test");

        // Assert
        result.Should().BeTrue();

        await serverTask;
    }

    [Fact]
    public async Task ExecuteShellCommandWithResponseAsync_NotConnected_ReturnsNull()
    {
        // Act
        var result = await _adbClient.ExecuteShellCommandWithResponseAsync("echo test");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteShellCommandWithResponseAsync_Connected_ReturnsCommandOutput()
    {
        // Arrange
        var port = GetAvailablePort();
        _tcpListener = new TcpListener(IPAddress.Loopback, port);
        _tcpListener.Start();

        var expectedOutput = "test output";
        
        var serverTask = Task.Run(async () =>
        {
            using var client = await _tcpListener.AcceptTcpClientAsync();
            using var stream = client.GetStream();
            
            // Handle connect
            var buffer = new byte[1024];
            _ = await stream.ReadAsync(buffer);
            
            var connectResponse = new AdbMessage
            {
                Command = AdbCommands.Connect,
                Arg0 = AdbConstants.Version,
                Arg1 = AdbConstants.MaxPayload,
                DataLength = 0,
                Magic = AdbCommands.Connect ^ 0xffffffff
            };
            await stream.WriteAsync(connectResponse.ToBytes());
            
            // Handle shell command
            _ = await stream.ReadAsync(buffer);
            
            var okayResponse = new AdbMessage
            {
                Command = AdbCommands.Okay,
                Arg0 = 1,
                Arg1 = 1,
                DataLength = 0,
                Magic = AdbCommands.Okay ^ 0xffffffff
            };
            await stream.WriteAsync(okayResponse.ToBytes());
            
            // Send data response
            var dataResponse = new AdbMessage
            {
                Command = AdbCommands.Write,
                Arg0 = 1,
                Arg1 = 1,
                Data = Encoding.UTF8.GetBytes(expectedOutput),
                DataLength = (uint)expectedOutput.Length,
                Magic = AdbCommands.Write ^ 0xffffffff
            };
            await stream.WriteAsync(dataResponse.ToBytes());
            
            // Handle close
            _ = await stream.ReadAsync(buffer);
        });

        await _adbClient.ConnectAsync("localhost", port, _cancellationTokenSource.Token);

        // Act
        var result = await _adbClient.ExecuteShellCommandWithResponseAsync("echo test");

        // Assert
        result.Should().Be(expectedOutput);

        await serverTask;
    }

    [Fact]
    public async Task DisconnectAsync_WhenConnected_ClosesConnection()
    {
        // Arrange
        var port = GetAvailablePort();
        _tcpListener = new TcpListener(IPAddress.Loopback, port);
        _tcpListener.Start();

        var serverTask = Task.Run(async () =>
        {
            using var client = await _tcpListener.AcceptTcpClientAsync();
            using var stream = client.GetStream();
            
            // Handle connect
            var buffer = new byte[1024];
            _ = await stream.ReadAsync(buffer);
            
            var connectResponse = new AdbMessage
            {
                Command = AdbCommands.Connect,
                Arg0 = AdbConstants.Version,
                Arg1 = AdbConstants.MaxPayload,
                DataLength = 0,
                Magic = AdbCommands.Connect ^ 0xffffffff
            };
            await stream.WriteAsync(connectResponse.ToBytes());
        });

        await _adbClient.ConnectAsync("localhost", port, _cancellationTokenSource.Token);
        _adbClient.IsConnected.Should().BeTrue();

        // Act
        await _adbClient.DisconnectAsync();

        // Assert
        _adbClient.IsConnected.Should().BeFalse();

        await serverTask;
    }

    [Fact]
    public void Dispose_WhenConnected_DisconnectsClient()
    {
        // Arrange & Act
        _adbClient.Dispose();

        // Assert
        _adbClient.IsConnected.Should().BeFalse();
    }

    private static int GetAvailablePort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        _tcpListener?.Stop();
        _adbClient.Dispose();
    }
}