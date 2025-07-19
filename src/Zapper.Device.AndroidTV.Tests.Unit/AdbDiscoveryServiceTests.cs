using System.Net;
using System.Net.Sockets;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Zapper.Device.AndroidTV.Models;

namespace Zapper.Device.AndroidTV.Tests.Unit;

public class AdbDiscoveryServiceTests : IDisposable
{
    private readonly AdbDiscoveryService _discoveryService;
    private readonly List<TcpListener> _tcpListeners = new();

    public AdbDiscoveryServiceTests()
    {
        var mockLogger = Substitute.For<ILogger<AdbDiscoveryService>>();
        var mockLoggerFactory = Substitute.For<ILoggerFactory>();
        var mockAdbClientLogger = Substitute.For<ILogger<AdbClient>>();

        mockLoggerFactory.CreateLogger<AdbClient>().Returns(mockAdbClientLogger);

        _discoveryService = new AdbDiscoveryService(mockLogger, mockLoggerFactory);
    }

    [Fact(Timeout = 3000)]
    public async Task TestDeviceAsync_ValidDevice_ReturnsTrue()
    {
        // Arrange
        var port = GetAvailablePort();
        var listener = new TcpListener(IPAddress.Loopback, port);
        _tcpListeners.Add(listener);
        listener.Start();

        var serverTask = Task.Run(async () =>
        {
            using var client = await listener.AcceptTcpClientAsync();
            await using var stream = client.GetStream();

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
        var result = await _discoveryService.TestDeviceAsync("localhost", port);

        // Assert
        result.Should().BeTrue();

        await serverTask;
    }

    [Fact]
    public async Task TestDeviceAsync_InvalidDevice_ReturnsFalse()
    {
        // Act
        var result = await _discoveryService.TestDeviceAsync("invalid.host.name");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task TestDeviceAsync_DeviceRequiresAuth_ReturnsFalse()
    {
        // Arrange
        var port = GetAvailablePort();
        var listener = new TcpListener(IPAddress.Loopback, port);
        _tcpListeners.Add(listener);
        listener.Start();

        var serverTask = Task.Run(async () =>
        {
            using var client = await listener.AcceptTcpClientAsync();
            using var stream = client.GetStream();

            // Read connect message
            var buffer = new byte[1024];
            _ = await stream.ReadAsync(buffer);

            // Send auth response
            var response = new AdbMessage
            {
                Command = AdbCommands.Auth,
                Arg0 = 1,
                Arg1 = 0,
                DataLength = 0,
                Magic = AdbCommands.Auth ^ 0xffffffff
            };

            await stream.WriteAsync(response.ToBytes());
        });

        // Act
        var result = await _discoveryService.TestDeviceAsync("localhost", port);

        // Assert
        result.Should().BeFalse();

        await serverTask;
    }

    [Fact]
    public async Task TestDeviceAsync_ConnectionTimeout_ReturnsFalse()
    {
        // Arrange
        var port = GetAvailablePort();
        // Don't start a listener, so connection will timeout

        // Act
        var result = await _discoveryService.TestDeviceAsync("localhost", port);

        // Assert
        result.Should().BeFalse();
    }

    [Fact(Skip = "AdbDiscoveryService performs actual network scanning which cannot be easily mocked in unit tests")]
    public async Task DiscoverDevicesAsync_NoDevicesFound_ReturnsEmptyList()
    {
        // This test is skipped because AdbDiscoveryService.DiscoverDevicesAsync() 
        // performs actual network operations that cannot be mocked without refactoring
        // the service to use dependency injection for network operations.

        // Act
        var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(1));
        var result = await _discoveryService.DiscoverDevicesAsync(cancellationTokenSource.Token);

        // Assert
        result.Should().BeEmpty();
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
        foreach (var listener in _tcpListeners)
        {
            listener.Stop();
        }
    }
}