using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using Zapper.Core.Models;

namespace Zapper.Device.WebOS.Tests.Unit;

public class WebOSProtocolControllerTests
{
    private readonly IWebOSDeviceController _mockWebOSController;
    private readonly ILogger<WebOSProtocolController> _mockLogger;
    private readonly WebOSProtocolController _controller;
    private readonly Zapper.Core.Models.Device _webOSDevice;
    private readonly DeviceCommand _testCommand;

    public WebOSProtocolControllerTests()
    {
        _mockWebOSController = Substitute.For<IWebOSDeviceController>();
        _mockLogger = Substitute.For<ILogger<WebOSProtocolController>>();
        _controller = new WebOSProtocolController(_mockWebOSController, _mockLogger);

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
            .SendCommandAsync(Arg.Any<Zapper.Core.Models.Device>(), Arg.Any<DeviceCommand>(), Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _controller.SendCommandAsync(_webOSDevice, _testCommand);

        // Assert
        result.Should().BeTrue();
        await _mockWebOSController.Received(1).SendCommandAsync(Arg.Any<Zapper.Core.Models.Device>(), Arg.Any<DeviceCommand>(), Arg.Any<CancellationToken>());
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
        await _mockWebOSController.DidNotReceive().SendCommandAsync(Arg.Any<Zapper.Core.Models.Device>(), Arg.Any<DeviceCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TestConnectionAsync_WithSupportedDevice_ShouldCallWebOSController()
    {
        // Arrange
        _mockWebOSController
            .TestConnectionAsync(Arg.Any<Zapper.Core.Models.Device>(), Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _controller.TestConnectionAsync(_webOSDevice);

        // Assert
        result.Should().BeTrue();
        await _mockWebOSController.Received(1).TestConnectionAsync(Arg.Any<Zapper.Core.Models.Device>(), Arg.Any<CancellationToken>());
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