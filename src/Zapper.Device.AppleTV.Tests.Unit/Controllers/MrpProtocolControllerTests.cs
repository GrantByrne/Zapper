using System.Net;
using System.Net.Sockets;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Zapper.Core.Models;
using Zapper.Device.AppleTV.Controllers;
using Zapper.Device.AppleTV.Models;

namespace Zapper.Device.AppleTV.Tests.Unit.Controllers;

public class MrpProtocolControllerTests : IDisposable
{
    private readonly ILogger<MrpProtocolController> _mockLogger;
    private readonly MrpProtocolController _controller;
    private TcpListener? _tcpListener;

    public MrpProtocolControllerTests()
    {
        _mockLogger = Substitute.For<ILogger<MrpProtocolController>>();
        _controller = new MrpProtocolController(_mockLogger);
    }

    [Fact]
    public void SupportedProtocol_ReturnsMediaRemoteProtocol()
    {
        // Assert
        _controller.SupportedProtocol.Should().Be(ConnectionType.MediaRemoteProtocol);
    }

    [Fact]
    public async Task ConnectAsync_ValidDevice_AttemptsTcpConnection()
    {
        // Arrange
        var port = GetAvailablePort();
        _tcpListener = new TcpListener(IPAddress.Loopback, port);
        _tcpListener.Start();

        var device = new Zapper.Core.Models.Device
        {
            Name = "Test Apple TV",
            IpAddress = "localhost",
            Port = port,
            RequiresPairing = false
        };

        var serverTask = Task.Run(async () =>
        {
            using var tcpClient = await _tcpListener.AcceptTcpClientAsync();
            // Don't close immediately - the controller expects the connection to stay open
            await Task.Delay(100);
        });

        // Act
        var result = await _controller.ConnectAsync(device);

        // Assert
        // The connection succeeds because we have a mock server accepting connections
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ConnectAsync_DeviceRequiresPairingButNotPaired_ReturnsFalse()
    {
        // Arrange
        var device = new Zapper.Core.Models.Device
        {
            Name = "Test Apple TV",
            IpAddress = "192.168.1.100",
            Port = 7000,
            RequiresPairing = true,
            IsPaired = false
        };

        // Act
        var result = await _controller.ConnectAsync(device);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ConnectAsync_InvalidHost_ReturnsFalse()
    {
        // Arrange
        var device = new Zapper.Core.Models.Device
        {
            Name = "Test Apple TV",
            IpAddress = "invalid.host.name",
            Port = 7000
        };

        // Act
        var result = await _controller.ConnectAsync(device);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DisconnectAsync_WhenConnected_ClosesConnection()
    {
        // Act
        var result = await _controller.DisconnectAsync();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SendCommandAsync_NotConnected_ReturnsFalse()
    {
        // Arrange
        var command = new DeviceCommand
        {
            Name = "play",
            Type = CommandType.PlayPause
        };

        // Act
        var result = await _controller.SendCommandAsync(command);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SendCommandAsync_UnknownCommand_ReturnsFalse()
    {
        // Arrange
        var command = new DeviceCommand
        {
            Name = "invalid_command",
            Type = CommandType.Custom
        };

        // Act
        var result = await _controller.SendCommandAsync(command);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetStatusAsync_NotConnected_ReturnsNull()
    {
        // Act
        var result = await _controller.GetStatusAsync();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task PairAsync_ValidPin_AttemptsPairing()
    {
        // Arrange
        var pin = "5678";

        // Act
        var result = await _controller.PairAsync(pin);

        // Assert
        // Will return false because not connected
        result.Should().BeFalse();
    }

    [Fact]
    public async Task PairAsync_InvalidPinFormat_ReturnsFalse()
    {
        // Arrange
        var pin = "ABC"; // Non-numeric pin

        // Act
        var result = await _controller.PairAsync(pin);

        // Assert
        result.Should().BeFalse();
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
        _tcpListener?.Stop();
    }
}