using Microsoft.Extensions.Logging;
using NSubstitute;
using Zapper.Core.Models;

namespace Zapper.Device.Sonos.Tests.Unit;

public class SonosProtocolControllerTests
{
    private readonly ISonosDeviceController _sonosControllerMock;
    private readonly ILogger<SonosProtocolController> _loggerMock;
    private readonly SonosProtocolController _controller;

    public SonosProtocolControllerTests()
    {
        _sonosControllerMock = Substitute.For<ISonosDeviceController>();
        _loggerMock = Substitute.For<ILogger<SonosProtocolController>>();
        _controller = new SonosProtocolController(_sonosControllerMock, _loggerMock);
    }

    [Fact]
    public async Task SendCommand_WithSonosDevice_CallsSonosController()
    {
        var device = new DeviceModel
        {
            Type = DeviceType.Sonos,
            ConnectionType = ConnectionType.Network,
            Name = "Test Sonos"
        };
        var command = new DeviceCommand { Type = CommandType.Power };
        _sonosControllerMock.SendCommand(device, command, Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _controller.SendCommand(device, command);

        Assert.True(result);
        await _sonosControllerMock.Received(1).SendCommand(device, command, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendCommand_WithNonSonosDevice_ReturnsFalse()
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
            Arg.Is<object>(v => v.ToString()!.Contains("not a Sonos device")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task TestConnection_WithSonosDevice_CallsSonosController()
    {
        var device = new DeviceModel
        {
            Type = DeviceType.Sonos,
            ConnectionType = ConnectionType.Network,
            Name = "Test Sonos"
        };
        _sonosControllerMock.TestConnection(device, Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _controller.TestConnection(device);

        Assert.True(result);
        await _sonosControllerMock.Received(1).TestConnection(device, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetStatus_WithSonosDevice_ReturnsOnlineStatus()
    {
        var device = new DeviceModel
        {
            Type = DeviceType.Sonos,
            ConnectionType = ConnectionType.Network,
            Name = "Test Sonos"
        };
        _sonosControllerMock.TestConnection(device, Arg.Any<CancellationToken>())
            .Returns(true);

        var status = await _controller.GetStatus(device);

        Assert.NotNull(status);
        Assert.True(status.IsOnline);
        Assert.Equal("Sonos speaker is online", status.StatusMessage);
    }

    [Fact]
    public void SupportsDevice_WithSonosNetworkDevice_ReturnsTrue()
    {
        var device = new DeviceModel
        {
            Type = DeviceType.Sonos,
            ConnectionType = ConnectionType.Network
        };

        var result = _controller.SupportsDevice(device);

        Assert.True(result);
    }

    [Fact]
    public void SupportsDevice_WithNonSonosDevice_ReturnsFalse()
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