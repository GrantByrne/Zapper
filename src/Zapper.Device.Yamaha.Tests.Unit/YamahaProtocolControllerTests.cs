using Microsoft.Extensions.Logging;
using Moq;
using Zapper.Core.Models;

namespace Zapper.Device.Yamaha.Tests.Unit;

public class YamahaProtocolControllerTests
{
    private readonly Mock<IYamahaDeviceController> _yamahaControllerMock;
    private readonly Mock<ILogger<YamahaProtocolController>> _loggerMock;
    private readonly YamahaProtocolController _controller;

    public YamahaProtocolControllerTests()
    {
        _yamahaControllerMock = new Mock<IYamahaDeviceController>();
        _loggerMock = new Mock<ILogger<YamahaProtocolController>>();
        _controller = new YamahaProtocolController(_yamahaControllerMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task SendCommand_WithYamahaDevice_CallsYamahaController()
    {
        var device = new DeviceModel
        {
            Type = DeviceType.YamahaReceiver,
            ConnectionType = ConnectionType.Network,
            Name = "Test Yamaha"
        };
        var command = new DeviceCommand { Type = CommandType.Power };
        _yamahaControllerMock.Setup(x => x.SendCommand(device, command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _controller.SendCommand(device, command);

        Assert.True(result);
        _yamahaControllerMock.Verify(x => x.SendCommand(device, command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendCommand_WithNonYamahaDevice_ReturnsFalse()
    {
        var device = new DeviceModel
        {
            Type = DeviceType.Television,
            ConnectionType = ConnectionType.Network,
            Name = "Test TV"
        };
        var command = new DeviceCommand { Type = CommandType.Power };

        var result = await _controller.SendCommand(device, command);

        Assert.False(result);
        _loggerMock.Verify(x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("not a Yamaha receiver")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task TestConnection_WithYamahaDevice_CallsYamahaController()
    {
        var device = new DeviceModel
        {
            Type = DeviceType.YamahaReceiver,
            ConnectionType = ConnectionType.Network,
            Name = "Test Yamaha"
        };
        _yamahaControllerMock.Setup(x => x.TestConnection(device, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _controller.TestConnection(device);

        Assert.True(result);
        _yamahaControllerMock.Verify(x => x.TestConnection(device, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetStatus_WithYamahaDevice_ReturnsOnlineStatus()
    {
        var device = new DeviceModel
        {
            Type = DeviceType.YamahaReceiver,
            ConnectionType = ConnectionType.Network,
            Name = "Test Yamaha"
        };
        _yamahaControllerMock.Setup(x => x.TestConnection(device, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var status = await _controller.GetStatus(device);

        Assert.NotNull(status);
        Assert.True(status.IsOnline);
        Assert.Equal("Yamaha receiver is online", status.StatusMessage);
    }

    [Fact]
    public void SupportsDevice_WithYamahaNetworkDevice_ReturnsTrue()
    {
        var device = new DeviceModel
        {
            Type = DeviceType.YamahaReceiver,
            ConnectionType = ConnectionType.Network
        };

        var result = _controller.SupportsDevice(device);

        Assert.True(result);
    }

    [Fact]
    public void SupportsDevice_WithNonYamahaDevice_ReturnsFalse()
    {
        var device = new DeviceModel
        {
            Type = DeviceType.Television,
            ConnectionType = ConnectionType.Network
        };

        var result = _controller.SupportsDevice(device);

        Assert.False(result);
    }
}