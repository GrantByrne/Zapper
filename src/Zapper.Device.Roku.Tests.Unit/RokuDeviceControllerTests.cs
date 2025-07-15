using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Zapper.Core.Models;
using Zapper.Device.Network;

namespace Zapper.Device.Roku.Tests.Unit;

public class RokuDeviceControllerTests
{
    private readonly INetworkDeviceController _mockNetworkController;
    private readonly ILogger<RokuDeviceController> _mockLogger;
    private readonly RokuDeviceController _controller;

    public RokuDeviceControllerTests()
    {
        _mockNetworkController = Substitute.For<INetworkDeviceController>();
        _mockLogger = Substitute.For<ILogger<RokuDeviceController>>();
        _controller = new RokuDeviceController(_mockNetworkController, _mockLogger);
    }

    [Fact]
    public async Task SendCommandAsync_PowerCommand_SendsCorrectKeypress()
    {
        // Arrange
        var device = new Zapper.Core.Models.Device
        {
            ConnectionType = ConnectionType.NetworkHttp,
            IpAddress = "192.168.1.100"
        };
        var command = new DeviceCommand { Type = CommandType.Power };

        _mockNetworkController
            .SendHttpCommandAsync("http://192.168.1.100:8060", "/keypress/Power", "POST", null, null, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _controller.SendCommandAsync(device, command);

        // Assert
        result.Should().BeTrue();
        await _mockNetworkController.Received(1).SendHttpCommandAsync("http://192.168.1.100:8060", "/keypress/Power", "POST", null, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TestConnectionAsync_ValidDevice_ReturnsTrue()
    {
        // Arrange
        var device = new Zapper.Core.Models.Device
        {
            IpAddress = "192.168.1.100"
        };

        _mockNetworkController
            .SendHttpCommandAsync("http://192.168.1.100:8060", "/", "GET", null, null, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _controller.TestConnectionAsync(device);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task LaunchAppAsync_ValidAppId_SendsCorrectRequest()
    {
        // Arrange
        var ipAddress = "192.168.1.100";
        var appId = "12"; // Netflix

        _mockNetworkController
            .SendHttpCommandAsync("http://192.168.1.100:8060", "/launch/12", "POST", null, null, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _controller.LaunchAppAsync(ipAddress, appId);

        // Assert
        result.Should().BeTrue();
        await _mockNetworkController.Received(1).SendHttpCommandAsync("http://192.168.1.100:8060", "/launch/12", "POST", null, null, Arg.Any<CancellationToken>());
    }
}