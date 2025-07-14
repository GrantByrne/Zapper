using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using Zapper.Core.Models;

namespace Zapper.Device.WebOS.Tests.Unit;

public class WebOSHardwareControllerTests
{
    private readonly IWebOSClient _mockWebOSClient;
    private readonly ILogger<WebOSHardwareController> _mockLogger;
    private readonly WebOSHardwareController _controller;
    private readonly Zapper.Core.Models.Device _webOSDevice;

    public WebOSHardwareControllerTests()
    {
        _mockWebOSClient = Substitute.For<IWebOSClient>();
        _mockLogger = Substitute.For<ILogger<WebOSHardwareController>>();
        _controller = new WebOSHardwareController(_mockWebOSClient, _mockLogger);

        _webOSDevice = new Zapper.Core.Models.Device
        {
            Id = 1,
            Name = "Test WebOS TV",
            ConnectionType = ConnectionType.WebOS,
            NetworkAddress = "192.168.1.100",
            AuthenticationToken = "test-client-key"
        };
    }

    [Fact]
    public async Task SendCommandAsync_WithPowerCommand_ShouldCallPowerOff()
    {
        // Arrange
        var powerCommand = new DeviceCommand
        {
            Name = "Power Off",
            Type = CommandType.Power
        };

        _mockWebOSClient.ConnectAsync(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                       .Returns(true);
        _mockWebOSClient.AuthenticateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                       .Returns(true);
        _mockWebOSClient.PowerOffAsync(Arg.Any<CancellationToken>())
                       .Returns(true);

        // Act
        var result = await _controller.SendCommandAsync(_webOSDevice, powerCommand);

        // Assert
        result.Should().BeTrue();
        await _mockWebOSClient.Received(1).PowerOffAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendCommandAsync_WithConnectionFailure_ShouldReturnFalse()
    {
        // Arrange
        var command = new DeviceCommand { Type = CommandType.Power };

        _mockWebOSClient.ConnectAsync(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                       .Returns(false);

        // Act
        var result = await _controller.SendCommandAsync(_webOSDevice, command);

        // Assert
        result.Should().BeFalse();
        await _mockWebOSClient.Received(1).ConnectAsync(_webOSDevice.NetworkAddress!, false, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TestConnectionAsync_WithSuccessfulConnection_ShouldReturnTrue()
    {
        // Arrange
        _mockWebOSClient.ConnectAsync(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                       .Returns(true);
        _mockWebOSClient.AuthenticateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                       .Returns(true);

        // Act
        var result = await _controller.TestConnectionAsync(_webOSDevice);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task TestConnectionAsync_WithConnectionFailure_ShouldReturnFalse()
    {
        // Arrange
        _mockWebOSClient.ConnectAsync(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                       .Returns(false);

        // Act
        var result = await _controller.TestConnectionAsync(_webOSDevice);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(CommandType.VolumeUp)]
    [InlineData(CommandType.VolumeDown)]
    [InlineData(CommandType.ChannelUp)]
    [InlineData(CommandType.ChannelDown)]
    public async Task SendCommandAsync_WithBasicCommands_ShouldCallCorrectMethods(CommandType commandType)
    {
        // Arrange
        var command = new DeviceCommand { Type = commandType };

        _mockWebOSClient.ConnectAsync(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                       .Returns(true);
        _mockWebOSClient.AuthenticateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                       .Returns(true);
        _mockWebOSClient.VolumeUpAsync(Arg.Any<CancellationToken>())
                       .Returns(true);
        _mockWebOSClient.VolumeDownAsync(Arg.Any<CancellationToken>())
                       .Returns(true);
        _mockWebOSClient.ChannelUpAsync(Arg.Any<CancellationToken>())
                       .Returns(true);
        _mockWebOSClient.ChannelDownAsync(Arg.Any<CancellationToken>())
                       .Returns(true);

        // Act
        var result = await _controller.SendCommandAsync(_webOSDevice, command);

        // Assert
        result.Should().BeTrue();

        switch (commandType)
        {
            case CommandType.VolumeUp:
                await _mockWebOSClient.Received(1).VolumeUpAsync(Arg.Any<CancellationToken>());
                break;
            case CommandType.VolumeDown:
                await _mockWebOSClient.Received(1).VolumeDownAsync(Arg.Any<CancellationToken>());
                break;
            case CommandType.ChannelUp:
                await _mockWebOSClient.Received(1).ChannelUpAsync(Arg.Any<CancellationToken>());
                break;
            case CommandType.ChannelDown:
                await _mockWebOSClient.Received(1).ChannelDownAsync(Arg.Any<CancellationToken>());
                break;
        }
    }
}