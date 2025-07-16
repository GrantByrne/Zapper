using Microsoft.Extensions.Logging;
using NSubstitute;
using Zapper.Core.Models;

namespace Zapper.Device.PlayStation.Tests.Unit;

public class PlayStationProtocolControllerTests
{
    private readonly IPlayStationDeviceController _playStationController;
    private readonly ILogger<PlayStationProtocolController> _logger;
    private readonly PlayStationProtocolController _controller;

    public PlayStationProtocolControllerTests()
    {
        _playStationController = Substitute.For<IPlayStationDeviceController>();
        _logger = Substitute.For<ILogger<PlayStationProtocolController>>();
        _controller = new PlayStationProtocolController(_playStationController, _logger);
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
        _playStationController.SendCommand(device, command, Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _controller.SendCommand(device, command);

        Assert.True(result);
        await _playStationController.Received(1).SendCommand(device, command, Arg.Any<CancellationToken>());
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
        _logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(v => v.ToString()!.Contains("not a PlayStation device")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
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
        _playStationController.TestConnection(device, Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _controller.TestConnection(device);

        Assert.True(result);
        await _playStationController.Received(1).TestConnection(device, Arg.Any<CancellationToken>());
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
        _playStationController.TestConnection(device, Arg.Any<CancellationToken>())
            .Returns(true);

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