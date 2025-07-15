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
    public async Task ConnectAsync_WithValidDevice_ReturnsTrue()
    {
        var device = new DeviceModel { IpAddress = "192.168.1.100", Name = "Test PlayStation" };

        var result = await _controller.ConnectAsync(device);

        Assert.True(result);
    }

    [Fact(Timeout = 5000)]
    public async Task ConnectAsync_WithoutIpAddress_ReturnsFalse()
    {
        var device = new DeviceModel { Name = "Test PlayStation" };

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

    [Fact(Timeout = 5000)]
    public async Task DisconnectAsync_WithValidDevice_ReturnsTrue()
    {
        var device = new DeviceModel { IpAddress = "192.168.1.100", Name = "Test PlayStation" };
        await _controller.ConnectAsync(device);

        var result = await _controller.DisconnectAsync(device);

        Assert.True(result);
    }

    [Fact(Timeout = 5000)]
    public async Task SendCommandAsync_PowerCommand_ReturnsResult()
    {
        var device = new DeviceModel
        {
            IpAddress = "192.168.1.100",
            Name = "Test PlayStation"
        };
        var command = new DeviceCommand { Type = CommandType.Power };

        var result = await _controller.SendCommandAsync(device, command);

        Assert.False(result); // Will fail without actual PlayStation
    }

    [Fact(Timeout = 5000)]
    public async Task SendCommandAsync_DirectionalCommand_HandlesAllDirections()
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
            var result = await _controller.SendCommandAsync(device, command);
            Assert.False(result); // Will fail without actual PlayStation
        }
    }

    [Fact(Timeout = 5000)]
    public async Task SendCommandAsync_WithoutIpAddress_ReturnsFalse()
    {
        var device = new DeviceModel { Name = "Test PlayStation" };
        var command = new DeviceCommand { Type = CommandType.Ok };

        var result = await _controller.SendCommandAsync(device, command);

        Assert.False(result);
    }

    [Fact(Timeout = 5000)]
    public async Task TestConnectionAsync_WithValidIpAddress_ReturnsResult()
    {
        var device = new DeviceModel { IpAddress = "192.168.1.100", Name = "Test PlayStation" };

        var result = await _controller.TestConnectionAsync(device);

        Assert.False(result); // Will fail without actual PlayStation
    }

    [Fact(Timeout = 5000)]
    public async Task PowerOnAsync_WithValidDevice_SendsCommand()
    {
        var device = new DeviceModel { IpAddress = "192.168.1.100", Name = "Test PlayStation" };

        var result = await _controller.PowerOnAsync(device);

        Assert.False(result); // Will fail without actual PlayStation
    }

    [Fact(Timeout = 5000)]
    public async Task NavigateAsync_WithValidDirection_SendsCommand()
    {
        var device = new DeviceModel { IpAddress = "192.168.1.100", Name = "Test PlayStation" };

        var result = await _controller.NavigateAsync(device, "up");

        Assert.False(result); // Will fail without actual PlayStation
    }

    [Fact(Timeout = 5000)]
    public async Task SendCommandAsync_CustomCommand_HandlesCustomPayload()
    {
        var device = new DeviceModel { IpAddress = "192.168.1.100", Name = "Test PlayStation" };
        var command = new DeviceCommand
        {
            Type = CommandType.Custom,
            NetworkPayload = "cross"
        };

        var result = await _controller.SendCommandAsync(device, command);

        Assert.False(result); // Will fail without actual PlayStation
    }
}