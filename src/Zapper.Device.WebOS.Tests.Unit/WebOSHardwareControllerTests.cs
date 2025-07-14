using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Zapper.Core.Models;

namespace Zapper.Device.WebOS.Tests.Unit;

public class WebOSHardwareControllerTests
{
    private readonly Mock<IWebOSClient> _mockWebOSClient;
    private readonly Mock<ILogger<WebOSHardwareController>> _mockLogger;
    private readonly WebOSHardwareController _controller;
    private readonly Zapper.Core.Models.Device _webOSDevice;

    public WebOSHardwareControllerTests()
    {
        _mockWebOSClient = new Mock<IWebOSClient>();
        _mockLogger = new Mock<ILogger<WebOSHardwareController>>();
        _controller = new WebOSHardwareController(_mockWebOSClient.Object, _mockLogger.Object);

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

        _mockWebOSClient.Setup(x => x.ConnectAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(true);
        _mockWebOSClient.Setup(x => x.AuthenticateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(true);
        _mockWebOSClient.Setup(x => x.PowerOffAsync(It.IsAny<CancellationToken>()))
                       .ReturnsAsync(true);

        // Act
        var result = await _controller.SendCommandAsync(_webOSDevice, powerCommand);

        // Assert
        result.Should().BeTrue();
        _mockWebOSClient.Verify(x => x.PowerOffAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendCommandAsync_WithConnectionFailure_ShouldReturnFalse()
    {
        // Arrange
        var command = new DeviceCommand { Type = CommandType.Power };

        _mockWebOSClient.Setup(x => x.ConnectAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(false);

        // Act
        var result = await _controller.SendCommandAsync(_webOSDevice, command);

        // Assert
        result.Should().BeFalse();
        _mockWebOSClient.Verify(x => x.ConnectAsync(_webOSDevice.NetworkAddress!, false, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TestConnectionAsync_WithSuccessfulConnection_ShouldReturnTrue()
    {
        // Arrange
        _mockWebOSClient.Setup(x => x.ConnectAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(true);
        _mockWebOSClient.Setup(x => x.AuthenticateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(true);

        // Act
        var result = await _controller.TestConnectionAsync(_webOSDevice);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task TestConnectionAsync_WithConnectionFailure_ShouldReturnFalse()
    {
        // Arrange
        _mockWebOSClient.Setup(x => x.ConnectAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(false);

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

        _mockWebOSClient.Setup(x => x.ConnectAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(true);
        _mockWebOSClient.Setup(x => x.AuthenticateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(true);
        _mockWebOSClient.Setup(x => x.VolumeUpAsync(It.IsAny<CancellationToken>()))
                       .ReturnsAsync(true);
        _mockWebOSClient.Setup(x => x.VolumeDownAsync(It.IsAny<CancellationToken>()))
                       .ReturnsAsync(true);
        _mockWebOSClient.Setup(x => x.ChannelUpAsync(It.IsAny<CancellationToken>()))
                       .ReturnsAsync(true);
        _mockWebOSClient.Setup(x => x.ChannelDownAsync(It.IsAny<CancellationToken>()))
                       .ReturnsAsync(true);

        // Act
        var result = await _controller.SendCommandAsync(_webOSDevice, command);

        // Assert
        result.Should().BeTrue();

        switch (commandType)
        {
            case CommandType.VolumeUp:
                _mockWebOSClient.Verify(x => x.VolumeUpAsync(It.IsAny<CancellationToken>()), Times.Once);
                break;
            case CommandType.VolumeDown:
                _mockWebOSClient.Verify(x => x.VolumeDownAsync(It.IsAny<CancellationToken>()), Times.Once);
                break;
            case CommandType.ChannelUp:
                _mockWebOSClient.Verify(x => x.ChannelUpAsync(It.IsAny<CancellationToken>()), Times.Once);
                break;
            case CommandType.ChannelDown:
                _mockWebOSClient.Verify(x => x.ChannelDownAsync(It.IsAny<CancellationToken>()), Times.Once);
                break;
        }
    }
}