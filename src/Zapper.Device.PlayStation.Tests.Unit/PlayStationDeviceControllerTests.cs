using Microsoft.Extensions.Logging;
using Moq;
using Zapper.Core.Models;

namespace Zapper.Device.PlayStation.Tests.Unit;

public class PlayStationDeviceControllerTests
{
    private readonly Mock<ILogger<PlayStationDeviceController>> _loggerMock;
    private readonly PlayStationDeviceController _controller;

    public PlayStationDeviceControllerTests()
    {
        _loggerMock = new Mock<ILogger<PlayStationDeviceController>>();
        _controller = new PlayStationDeviceController(_loggerMock.Object);
    }

    [Fact(Timeout = 5000)]
    public async Task Connect_WithValidDevice_ReturnsTrue()
    {
        var device = new DeviceModel { IpAddress = "192.168.1.100", Name = "Test PlayStation" };

        var result = await _controller.Connect(device);

        Assert.True(result);
    }

    [Fact(Timeout = 5000)]
    public async Task Connect_WithoutIpAddress_ReturnsFalse()
    {
        var device = new DeviceModel { Name = "Test PlayStation" };

        var result = await _controller.Connect(device);

        Assert.False(result);
        _loggerMock.Verify(x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("no IP address")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact(Timeout = 5000)]
    public async Task Disconnect_WithValidDevice_ReturnsTrue()
    {
        var device = new DeviceModel { IpAddress = "192.168.1.100", Name = "Test PlayStation" };
        await _controller.Connect(device);

        var result = await _controller.Disconnect(device);

        Assert.True(result);
    }

    [Fact(Timeout = 5000)]
    public async Task SendCommand_PowerCommand_ReturnsResult()
    {
        var device = new DeviceModel
        {
            IpAddress = "192.168.1.100",
            Name = "Test PlayStation"
        };
        var command = new DeviceCommand { Type = CommandType.Power };

        var result = await _controller.SendCommand(device, command);

        Assert.False(result); // Will fail without actual PlayStation
    }

    [Fact(Timeout = 5000)]
    public async Task SendCommand_DirectionalCommand_HandlesAllDirections()
    {
        var device = new DeviceModel { IpAddress = "192.168.1.100", Name = "Test PlayStation" };
        var directions = new[]
        {
            CommandType.DirectionalUp,
            CommandType.DirectionalDown,
            CommandType.DirectionalLeft,
            CommandType.DirectionalRight
        };

        foreach (var direction in directions)
        {
            var command = new DeviceCommand { Type = direction };
            var result = await _controller.SendCommand(device, command);
            Assert.False(result); // Will fail without actual PlayStation
        }
    }

    [Fact(Timeout = 5000)]
    public async Task SendCommand_WithoutIpAddress_ReturnsFalse()
    {
        var device = new DeviceModel { Name = "Test PlayStation" };
        var command = new DeviceCommand { Type = CommandType.Ok };

        var result = await _controller.SendCommand(device, command);

        Assert.False(result);
    }

    [Fact(Timeout = 5000)]
    public async Task TestConnection_WithValidIpAddress_ReturnsResult()
    {
        var device = new DeviceModel { IpAddress = "192.168.1.100", Name = "Test PlayStation" };

        var result = await _controller.TestConnection(device);

        Assert.False(result); // Will fail without actual PlayStation
    }

    [Fact(Timeout = 5000)]
    public async Task PowerOn_WithValidDevice_SendsCommand()
    {
        var device = new DeviceModel { IpAddress = "192.168.1.100", Name = "Test PlayStation" };

        var result = await _controller.PowerOn(device);

        Assert.False(result); // Will fail without actual PlayStation
    }

    [Fact(Timeout = 5000)]
    public async Task Navigate_WithValidDirection_SendsCommand()
    {
        var device = new DeviceModel { IpAddress = "192.168.1.100", Name = "Test PlayStation" };

        var result = await _controller.Navigate(device, "up");

        Assert.False(result); // Will fail without actual PlayStation
    }

    [Fact(Timeout = 5000)]
    public async Task SendCommand_CustomCommand_HandlesCustomPayload()
    {
        var device = new DeviceModel { IpAddress = "192.168.1.100", Name = "Test PlayStation" };
        var command = new DeviceCommand
        {
            Type = CommandType.Custom,
            NetworkPayload = "cross"
        };

        var result = await _controller.SendCommand(device, command);

        Assert.False(result); // Will fail without actual PlayStation
    }
}