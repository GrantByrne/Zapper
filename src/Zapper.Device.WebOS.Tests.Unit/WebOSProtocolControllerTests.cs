using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using Zapper.Core.Models;

namespace Zapper.Device.WebOS.Tests.Unit;

public class WebOsProtocolControllerTests
{
    private readonly IWebOsDeviceController _mockWebOsController;
    private readonly ILogger<WebOsProtocolController> _mockLogger;
    private readonly WebOsProtocolController _controller;
    private readonly Zapper.Core.Models.Device _webOsDevice;
    private readonly DeviceCommand _testCommand;

    public WebOsProtocolControllerTests()
    {
        _mockWebOsController = Substitute.For<IWebOsDeviceController>();
        _mockLogger = Substitute.For<ILogger<WebOsProtocolController>>();
        _controller = new WebOsProtocolController(_mockWebOsController, _mockLogger);

        _webOsDevice = new Zapper.Core.Models.Device
        {
            Id = 1,
            Name = "Test WebOS TV",
            ConnectionType = ConnectionType.WebOs,
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
        _mockWebOsController
            .SendCommandAsync(Arg.Any<Zapper.Core.Models.Device>(), Arg.Any<DeviceCommand>(), Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _controller.SendCommandAsync(_webOsDevice, _testCommand);

        // Assert
        result.Should().BeTrue();
        await _mockWebOsController.Received(1).SendCommandAsync(Arg.Any<Zapper.Core.Models.Device>(), Arg.Any<DeviceCommand>(), Arg.Any<CancellationToken>());
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
        await _mockWebOsController.DidNotReceive().SendCommandAsync(Arg.Any<Zapper.Core.Models.Device>(), Arg.Any<DeviceCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TestConnectionAsync_WithSupportedDevice_ShouldCallWebOSController()
    {
        // Arrange
        _mockWebOsController
            .TestConnectionAsync(Arg.Any<Zapper.Core.Models.Device>(), Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _controller.TestConnectionAsync(_webOsDevice);

        // Assert
        result.Should().BeTrue();
        await _mockWebOsController.Received(1).TestConnectionAsync(Arg.Any<Zapper.Core.Models.Device>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(ConnectionType.WebOs, true)]
    [InlineData(ConnectionType.Bluetooth, false)]
    [InlineData(ConnectionType.InfraredIr, false)]
    [InlineData(ConnectionType.NetworkTcp, false)]
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