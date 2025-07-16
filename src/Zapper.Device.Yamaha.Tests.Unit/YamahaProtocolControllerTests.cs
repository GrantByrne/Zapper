using Microsoft.Extensions.Logging;
using NSubstitute;
using Zapper.Core.Models;

namespace Zapper.Device.Yamaha.Tests.Unit;

public class YamahaProtocolControllerTests
{
    private readonly IYamahaDeviceController _yamahaControllerMock;
    private readonly ILogger<YamahaProtocolController> _loggerMock;
    private readonly YamahaProtocolController _controller;

    public YamahaProtocolControllerTests()
    {
        _yamahaControllerMock = Substitute.For<IYamahaDeviceController>();
        _loggerMock = Substitute.For<ILogger<YamahaProtocolController>>();
        _controller = new YamahaProtocolController(_yamahaControllerMock, _loggerMock);
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
        _yamahaControllerMock.SendCommand(device, command, Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _controller.SendCommand(device, command);

        Assert.True(result);
        await _yamahaControllerMock.Received(1).SendCommand(device, command, Arg.Any<CancellationToken>());
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
        _loggerMock.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(v => v.ToString()!.Contains("not a Yamaha receiver")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
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
        _yamahaControllerMock.TestConnection(device, Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _controller.TestConnection(device);

        Assert.True(result);
        await _yamahaControllerMock.Received(1).TestConnection(device, Arg.Any<CancellationToken>());
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
        _yamahaControllerMock.TestConnection(device, Arg.Any<CancellationToken>())
            .Returns(true);

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