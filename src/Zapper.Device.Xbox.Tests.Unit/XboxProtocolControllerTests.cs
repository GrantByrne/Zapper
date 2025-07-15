using Microsoft.Extensions.Logging;
using Moq;

namespace Zapper.Device.Xbox.Tests.Unit;

public class XboxProtocolControllerTests
{
    private readonly Mock<IXboxDeviceController> _xboxControllerMock;
    private readonly Mock<ILogger<XboxProtocolController>> _loggerMock;
    private readonly XboxProtocolController _controller;

    public XboxProtocolControllerTests()
    {
        _xboxControllerMock = new Mock<IXboxDeviceController>();
        _loggerMock = new Mock<ILogger<XboxProtocolController>>();
        _controller = new XboxProtocolController(_xboxControllerMock.Object, _loggerMock.Object);
    }

    [Fact]
    public void SupportsDevice_WithXboxDevice_ReturnsTrue()
    {
        var device = new Zapper.Core.Models.Device
        {
            Type = Core.Models.DeviceType.Xbox,
            ConnectionType = Core.Models.ConnectionType.Network
        };

        Assert.True(_controller.SupportsDevice(device));
    }

    [Fact]
    public void SupportsDevice_WithNonXboxDevice_ReturnsFalse()
    {
        var device = new Zapper.Core.Models.Device
        {
            Type = Core.Models.DeviceType.Television,
            ConnectionType = Core.Models.ConnectionType.Network
        };

        Assert.False(_controller.SupportsDevice(device));
    }

    [Fact(Timeout = 5000)]
    public async Task SendCommand_WithXboxDevice_CallsXboxController()
    {
        var device = new DeviceModel
        {
            Type = Core.Models.DeviceType.Xbox,
            ConnectionType = Core.Models.ConnectionType.Network,
            IpAddress = "192.168.1.100"
        };
        var command = new Zapper.Core.Models.DeviceCommand { Type = Core.Models.CommandType.Ok };

        _xboxControllerMock.Setup(x => x.SendCommand(device, command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _controller.SendCommand(device, command);

        Assert.True(result);
        _xboxControllerMock.Verify(x => x.SendCommand(device, command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(Timeout = 5000)]
    public async Task SendCommand_WithNonXboxDevice_ReturnsFalse()
    {
        var device = new DeviceModel
        {
            Type = Core.Models.DeviceType.Television,
            ConnectionType = Core.Models.ConnectionType.Network
        };
        var command = new Zapper.Core.Models.DeviceCommand { Type = Core.Models.CommandType.Ok };

        var result = await _controller.SendCommand(device, command);

        Assert.False(result);
        _xboxControllerMock.Verify(x => x.SendCommand(It.IsAny<Zapper.Core.Models.Device>(), It.IsAny<Zapper.Core.Models.DeviceCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(Timeout = 5000)]
    public async Task TestConnection_WithXboxDevice_CallsXboxController()
    {
        var device = new DeviceModel
        {
            Type = Core.Models.DeviceType.Xbox,
            ConnectionType = Core.Models.ConnectionType.Network,
            IpAddress = "192.168.1.100"
        };

        _xboxControllerMock.Setup(x => x.TestConnection(device, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _controller.TestConnection(device);

        Assert.True(result);
        _xboxControllerMock.Verify(x => x.TestConnection(device, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(Timeout = 5000)]
    public async Task GetStatus_WithXboxDevice_ReturnsOnlineStatus()
    {
        var device = new DeviceModel
        {
            Type = Core.Models.DeviceType.Xbox,
            ConnectionType = Core.Models.ConnectionType.Network,
            IpAddress = "192.168.1.100"
        };

        _xboxControllerMock.Setup(x => x.TestConnection(device, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _controller.GetStatus(device);

        Assert.True(result.IsOnline);
        Assert.Equal("Xbox is online", result.StatusMessage);
    }

    [Fact(Timeout = 5000)]
    public async Task GetStatus_WithNonXboxDevice_ReturnsOfflineStatus()
    {
        var device = new DeviceModel
        {
            Type = Core.Models.DeviceType.Television,
            ConnectionType = Core.Models.ConnectionType.Network
        };

        var result = await _controller.GetStatus(device);

        Assert.False(result.IsOnline);
        Assert.Equal("Device is not an Xbox", result.StatusMessage);
    }
}