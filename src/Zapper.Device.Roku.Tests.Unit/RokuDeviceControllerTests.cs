using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Zapper.Core.Models;
using Zapper.Device.Network;

namespace Zapper.Device.Roku.Tests.Unit;

public class RokuDeviceControllerTests
{
    private readonly Mock<INetworkDeviceController> _mockNetworkController;
    private readonly Mock<ILogger<RokuDeviceController>> _mockLogger;
    private readonly RokuDeviceController _controller;

    public RokuDeviceControllerTests()
    {
        _mockNetworkController = new Mock<INetworkDeviceController>();
        _mockLogger = new Mock<ILogger<RokuDeviceController>>();
        _controller = new RokuDeviceController(_mockNetworkController.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task SendCommandAsync_PowerCommand_SendsCorrectKeypress()
    {
        // Arrange
        var device = new Zapper.Core.Models.Device
        {
            ConnectionType = ConnectionType.NetworkHTTP,
            IpAddress = "192.168.1.100"
        };
        var command = new DeviceCommand { Type = CommandType.Power };

        _mockNetworkController
            .Setup(x => x.SendHttpCommandAsync("http://192.168.1.100:8060", "/keypress/Power", "POST", null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.SendCommandAsync(device, command);

        // Assert
        Assert.True(result);
        _mockNetworkController.Verify(x => x.SendHttpCommandAsync("http://192.168.1.100:8060", "/keypress/Power", "POST", null, null, It.IsAny<CancellationToken>()), Times.Once);
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
            .Setup(x => x.SendHttpCommandAsync("http://192.168.1.100:8060", "/", "GET", null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.TestConnectionAsync(device);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task LaunchAppAsync_ValidAppId_SendsCorrectRequest()
    {
        // Arrange
        var ipAddress = "192.168.1.100";
        var appId = "12"; // Netflix

        _mockNetworkController
            .Setup(x => x.SendHttpCommandAsync("http://192.168.1.100:8060", "/launch/12", "POST", null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.LaunchAppAsync(ipAddress, appId);

        // Assert
        Assert.True(result);
        _mockNetworkController.Verify(x => x.SendHttpCommandAsync("http://192.168.1.100:8060", "/launch/12", "POST", null, null, It.IsAny<CancellationToken>()), Times.Once);
    }
}