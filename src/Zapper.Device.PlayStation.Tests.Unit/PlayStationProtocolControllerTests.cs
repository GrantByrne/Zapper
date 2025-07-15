using Microsoft.Extensions.Logging;
using Moq;
using Zapper.Core.Models;

namespace Zapper.Device.PlayStation.Tests.Unit;

public class PlayStationProtocolControllerTests
{
    private readonly Mock<IPlayStationDeviceController> _playStationControllerMock;
    private readonly Mock<ILogger<PlayStationProtocolController>> _loggerMock;
    private readonly PlayStationProtocolController _controller;

    public PlayStationProtocolControllerTests()
    {
        _playStationControllerMock = new Mock<IPlayStationDeviceController>();
        _loggerMock = new Mock<ILogger<PlayStationProtocolController>>();
        _controller = new PlayStationProtocolController(_playStationControllerMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task SendCommand_WithPlayStationDevice_CallsPlayStationController()
    {
        var device = new DeviceModel
        {
            Type = DeviceType.PlayStation,
            ConnectionType = ConnectionType.Network,
            Name = "Test PlayStation"
        };
        var command = new DeviceCommand { Type = CommandType.Power };
        _playStationControllerMock.Setup(x => x.SendCommand(device, command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _controller.SendCommand(device, command);

        Assert.True(result);
        _playStationControllerMock.Verify(x => x.SendCommand(device, command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendCommand_WithNonPlayStationDevice_ReturnsFalse()
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
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("not a PlayStation device")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task TestConnection_WithPlayStationDevice_CallsPlayStationController()
    {
        var device = new DeviceModel
        {
            Type = DeviceType.PlayStation,
            ConnectionType = ConnectionType.Network,
            Name = "Test PlayStation"
        };
        _playStationControllerMock.Setup(x => x.TestConnection(device, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _controller.TestConnection(device);

        Assert.True(result);
        _playStationControllerMock.Verify(x => x.TestConnection(device, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetStatus_WithPlayStationDevice_ReturnsOnlineStatus()
    {
        var device = new DeviceModel
        {
            Type = DeviceType.PlayStation,
            ConnectionType = ConnectionType.Network,
            Name = "Test PlayStation"
        };
        _playStationControllerMock.Setup(x => x.TestConnection(device, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var status = await _controller.GetStatus(device);

        Assert.NotNull(status);
        Assert.True(status.IsOnline);
        Assert.Equal("PlayStation is online", status.StatusMessage);
    }

    [Fact]
    public void SupportsDevice_WithPlayStationNetworkDevice_ReturnsTrue()
    {
        var device = new DeviceModel
        {
            Type = DeviceType.PlayStation,
            ConnectionType = ConnectionType.Network
        };

        var result = _controller.SupportsDevice(device);

        Assert.True(result);
    }

    [Fact]
    public void SupportsDevice_WithNonPlayStationDevice_ReturnsFalse()
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