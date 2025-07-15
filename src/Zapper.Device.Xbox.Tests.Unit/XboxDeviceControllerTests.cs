using Microsoft.Extensions.Logging;
using Moq;
using Zapper.Device.Network;

namespace Zapper.Device.Xbox.Tests.Unit;

public class XboxDeviceControllerTests
{
    private readonly Mock<INetworkDeviceController> _networkControllerMock;
    private readonly Mock<ILogger<XboxDeviceController>> _loggerMock;
    private readonly XboxDeviceController _controller;

    public XboxDeviceControllerTests()
    {
        _networkControllerMock = new Mock<INetworkDeviceController>();
        _loggerMock = new Mock<ILogger<XboxDeviceController>>();
        _controller = new XboxDeviceController(_networkControllerMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ConnectAsync_WithValidDevice_ReturnsTrue()
    {
        var device = new Zapper.Core.Models.Device { IpAddress = "192.168.1.100", Name = "Test Xbox" };

        var result = await _controller.ConnectAsync(device);

        Assert.True(result);
    }

    [Fact]
    public async Task ConnectAsync_WithoutIpAddress_ReturnsFalse()
    {
        var device = new Zapper.Core.Models.Device { Name = "Test Xbox" };

        var result = await _controller.ConnectAsync(device);

        Assert.False(result);
        _loggerMock.Verify(x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("no IP address")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task DisconnectAsync_WithValidDevice_ReturnsTrue()
    {
        var device = new Zapper.Core.Models.Device { IpAddress = "192.168.1.100", Name = "Test Xbox" };
        await _controller.ConnectAsync(device);

        var result = await _controller.DisconnectAsync(device);

        Assert.True(result);
    }

    [Fact]
    public async Task SendCommandAsync_PowerCommand_ReturnsResult()
    {
        var device = new Zapper.Core.Models.Device 
        { 
            IpAddress = "192.168.1.100", 
            Name = "Test Xbox",
            AuthToken = "test-live-id"
        };
        var command = new Zapper.Core.Models.DeviceCommand { Type = Zapper.Core.Models.CommandType.Power };

        var result = await _controller.SendCommandAsync(device, command);

        Assert.False(result); // Will fail without actual Xbox
    }

    [Fact]
    public async Task SendCommandAsync_DirectionalCommand_HandlesAllDirections()
    {
        var device = new Zapper.Core.Models.Device { IpAddress = "192.168.1.100", Name = "Test Xbox" };
        var directions = new[] 
        { 
            Zapper.Core.Models.CommandType.DirectionalUp, 
            Zapper.Core.Models.CommandType.DirectionalDown, 
            Zapper.Core.Models.CommandType.DirectionalLeft, 
            Zapper.Core.Models.CommandType.DirectionalRight 
        };

        foreach (var direction in directions)
        {
            var command = new Zapper.Core.Models.DeviceCommand { Type = direction };
            var result = await _controller.SendCommandAsync(device, command);
            Assert.False(result); // Will fail without actual Xbox
        }
    }

    [Fact]
    public async Task SendCommandAsync_WithoutIpAddress_ReturnsFalse()
    {
        var device = new Zapper.Core.Models.Device { Name = "Test Xbox" };
        var command = new Zapper.Core.Models.DeviceCommand { Type = Zapper.Core.Models.CommandType.Ok };

        var result = await _controller.SendCommandAsync(device, command);

        Assert.False(result);
    }

    [Fact]
    public async Task TestConnectionAsync_WithValidIpAddress_ReturnsResult()
    {
        var device = new Zapper.Core.Models.Device { IpAddress = "192.168.1.100", Name = "Test Xbox" };

        var result = await _controller.TestConnectionAsync(device);

        Assert.False(result); // Will fail without actual Xbox
    }

    [Fact]
    public async Task PowerOnAsync_WithoutLiveId_ReturnsFalse()
    {
        var device = new Zapper.Core.Models.Device { IpAddress = "192.168.1.100", Name = "Test Xbox" };

        var result = await _controller.PowerOnAsync(device);

        Assert.False(result);
        _loggerMock.Verify(x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("missing IP or Live ID")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendTextAsync_WithValidDevice_SendsText()
    {
        var device = new Zapper.Core.Models.Device { IpAddress = "192.168.1.100", Name = "Test Xbox" };
        var text = "Hello Xbox";

        var result = await _controller.SendTextAsync(device, text);

        Assert.False(result); // Will fail without actual Xbox
    }
}