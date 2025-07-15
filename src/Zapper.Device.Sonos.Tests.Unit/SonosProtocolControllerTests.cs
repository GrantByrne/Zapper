using Microsoft.Extensions.Logging;
using Moq;
using Zapper.Core.Interfaces;
using Zapper.Core.Models;
using Device = Zapper.Core.Models.Device;

namespace Zapper.Device.Sonos.Tests.Unit;

public class SonosProtocolControllerTests
{
    private readonly Mock<ISonosDeviceController> _sonosControllerMock;
    private readonly Mock<ILogger<SonosProtocolController>> _loggerMock;
    private readonly SonosProtocolController _controller;

    public SonosProtocolControllerTests()
    {
        _sonosControllerMock = new Mock<ISonosDeviceController>();
        _loggerMock = new Mock<ILogger<SonosProtocolController>>();
        _controller = new SonosProtocolController(_sonosControllerMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task SendCommandAsync_WithSonosDevice_CallsSonosController()
    {
        var device = new DeviceModel
        {
            Type = DeviceType.Sonos,
            ConnectionType = ConnectionType.Network,
            Name = "Test Sonos"
        };
        var command = new DeviceCommand { Type = CommandType.Power };
        _sonosControllerMock.Setup(x => x.SendCommandAsync(device, command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _controller.SendCommandAsync(device, command);

        Assert.True(result);
        _sonosControllerMock.Verify(x => x.SendCommandAsync(device, command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendCommandAsync_WithNonSonosDevice_ReturnsFalse()
    {
        var device = new DeviceModel
        {
            Type = DeviceType.Television,
            ConnectionType = ConnectionType.Network,
            Name = "Test TV"
        };
        var command = new DeviceCommand { Type = CommandType.Power };

        var result = await _controller.SendCommandAsync(device, command);

        Assert.False(result);
        _loggerMock.Verify(x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("not a Sonos device")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task TestConnectionAsync_WithSonosDevice_CallsSonosController()
    {
        var device = new DeviceModel
        {
            Type = DeviceType.Sonos,
            ConnectionType = ConnectionType.Network,
            Name = "Test Sonos"
        };
        _sonosControllerMock.Setup(x => x.TestConnectionAsync(device, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _controller.TestConnectionAsync(device);

        Assert.True(result);
        _sonosControllerMock.Verify(x => x.TestConnectionAsync(device, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetStatusAsync_WithSonosDevice_ReturnsOnlineStatus()
    {
        var device = new DeviceModel
        {
            Type = DeviceType.Sonos,
            ConnectionType = ConnectionType.Network,
            Name = "Test Sonos"
        };
        _sonosControllerMock.Setup(x => x.TestConnectionAsync(device, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var status = await _controller.GetStatusAsync(device);

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