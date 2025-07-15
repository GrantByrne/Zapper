using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using Zapper.Core.Models;

namespace Zapper.Device.WebOS.Tests.Unit;

public class WebOsHardwareControllerTests
{
    private readonly IWebOsClient _mockWebOsClient;
    private readonly ILogger<WebOsHardwareController> _mockLogger;
    private readonly WebOsHardwareController _controller;
    private readonly Zapper.Core.Models.Device _webOsDevice;

    public WebOsHardwareControllerTests()
    {
        _mockWebOsClient = Substitute.For<IWebOsClient>();
        _mockLogger = Substitute.For<ILogger<WebOsHardwareController>>();
        _controller = new WebOsHardwareController(_mockWebOsClient, _mockLogger);

        _webOsDevice = new Zapper.Core.Models.Device
        {
            Id = 1,
            Name = "Test WebOS TV",
            ConnectionType = ConnectionType.WebOs,
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

        _mockWebOsClient.ConnectAsync(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                       .Returns(true);
        _mockWebOsClient.AuthenticateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                       .Returns(true);
        _mockWebOsClient.PowerOffAsync(Arg.Any<CancellationToken>())
                       .Returns(true);

        // Act
        var result = await _controller.SendCommandAsync(_webOsDevice, powerCommand);

        // Assert
        result.Should().BeTrue();
        await _mockWebOsClient.Received(1).PowerOffAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendCommandAsync_WithConnectionFailure_ShouldReturnFalse()
    {
        // Arrange
        var command = new DeviceCommand { Type = CommandType.Power };

        _mockWebOsClient.ConnectAsync(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                       .Returns(false);

        // Act
        var result = await _controller.SendCommandAsync(_webOsDevice, command);

        // Assert
        result.Should().BeFalse();
        await _mockWebOsClient.Received(1).ConnectAsync(_webOsDevice.NetworkAddress!, false, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TestConnectionAsync_WithSuccessfulConnection_ShouldReturnTrue()
    {
        // Arrange
        _mockWebOsClient.ConnectAsync(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                       .Returns(true);
        _mockWebOsClient.AuthenticateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                       .Returns(true);

        // Act
        var result = await _controller.TestConnectionAsync(_webOsDevice);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task TestConnectionAsync_WithConnectionFailure_ShouldReturnFalse()
    {
        // Arrange
        _mockWebOsClient.ConnectAsync(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                       .Returns(false);

        // Act
        var result = await _controller.TestConnectionAsync(_webOsDevice);

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

        _mockWebOsClient.ConnectAsync(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                       .Returns(true);
        _mockWebOsClient.AuthenticateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                       .Returns(true);
        _mockWebOsClient.VolumeUpAsync(Arg.Any<CancellationToken>())
                       .Returns(true);
        _mockWebOsClient.VolumeDownAsync(Arg.Any<CancellationToken>())
                       .Returns(true);
        _mockWebOsClient.ChannelUpAsync(Arg.Any<CancellationToken>())
                       .Returns(true);
        _mockWebOsClient.ChannelDownAsync(Arg.Any<CancellationToken>())
                       .Returns(true);

        // Act
        var result = await _controller.SendCommandAsync(_webOsDevice, command);

        // Assert
        result.Should().BeTrue();

        switch (commandType)
        {
            case CommandType.VolumeUp:
                await _mockWebOsClient.Received(1).VolumeUpAsync(Arg.Any<CancellationToken>());
                break;
            case CommandType.VolumeDown:
                await _mockWebOsClient.Received(1).VolumeDownAsync(Arg.Any<CancellationToken>());
                break;
            case CommandType.ChannelUp:
                await _mockWebOsClient.Received(1).ChannelUpAsync(Arg.Any<CancellationToken>());
                break;
            case CommandType.ChannelDown:
                await _mockWebOsClient.Received(1).ChannelDownAsync(Arg.Any<CancellationToken>());
                break;
        }
    }
}