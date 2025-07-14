using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Zapper.Core.Models;

namespace Zapper.Device.WebOS.Tests.Unit;

public class WebOSProtocolControllerTests
{
    private readonly Mock<IWebOSDeviceController> _mockWebOSController;
    private readonly Mock<ILogger<WebOSProtocolController>> _mockLogger;
    private readonly WebOSProtocolController _controller;
    private readonly Zapper.Core.Models.Device _webOSDevice;
    private readonly DeviceCommand _testCommand;

    public WebOSProtocolControllerTests()
    {
        _mockWebOSController = new Mock<IWebOSDeviceController>();
        _mockLogger = new Mock<ILogger<WebOSProtocolController>>();
        _controller = new WebOSProtocolController(_mockWebOSController.Object, _mockLogger.Object);

        _webOSDevice = new Zapper.Core.Models.Device
        {
            Id = 1,
            Name = "Test WebOS TV",
            ConnectionType = ConnectionType.WebOS,
            NetworkAddress = "192.168.1.100"
        };

        _testCommand = new DeviceCommand
        {
            Id = 1,
            Name = "Power Off",
            Type = CommandType.Power
        };
    }

    [Fact]
    public async Task SendCommandAsync_WithSupportedDevice_ShouldCallWebOSController()
    {
        // Arrange
        _mockWebOSController
            .Setup(x => x.SendCommandAsync(It.IsAny<Zapper.Core.Models.Device>(), It.IsAny<DeviceCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.SendCommandAsync(_webOSDevice, _testCommand);

        // Assert
        result.Should().BeTrue();
        _mockWebOSController.Verify(x => x.SendCommandAsync(It.IsAny<Zapper.Core.Models.Device>(), It.IsAny<DeviceCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendCommandAsync_WithUnsupportedDevice_ShouldReturnFalse()
    {
        // Arrange
        var unsupportedDevice = new Zapper.Core.Models.Device
        {
            Id = 2,
            ConnectionType = ConnectionType.Bluetooth
        };

        // Act
        var result = await _controller.SendCommandAsync(unsupportedDevice, _testCommand);

        // Assert
        result.Should().BeFalse();
        _mockWebOSController.Verify(x => x.SendCommandAsync(It.IsAny<Zapper.Core.Models.Device>(), It.IsAny<DeviceCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task TestConnectionAsync_WithSupportedDevice_ShouldCallWebOSController()
    {
        // Arrange
        _mockWebOSController
            .Setup(x => x.TestConnectionAsync(It.IsAny<Zapper.Core.Models.Device>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.TestConnectionAsync(_webOSDevice);

        // Assert
        result.Should().BeTrue();
        _mockWebOSController.Verify(x => x.TestConnectionAsync(It.IsAny<Zapper.Core.Models.Device>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(ConnectionType.WebOS, true)]
    [InlineData(ConnectionType.Bluetooth, false)]
    [InlineData(ConnectionType.InfraredIR, false)]
    [InlineData(ConnectionType.NetworkTCP, false)]
    public void SupportsDevice_ShouldReturnCorrectValue(ConnectionType connectionType, bool expected)
    {
        // Arrange
        var device = new Zapper.Core.Models.Device { ConnectionType = connectionType };

        // Act
        var result = _controller.SupportsDevice(device);

        // Assert
        result.Should().Be(expected);
    }
}