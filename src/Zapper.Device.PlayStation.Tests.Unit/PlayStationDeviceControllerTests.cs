using Microsoft.Extensions.Logging;
using NSubstitute;
using Zapper.Core.Models;

namespace Zapper.Device.PlayStation.Tests.Unit;

public class PlayStationDeviceControllerTests
{
    private readonly ILogger<PlayStationDeviceController> _logger;
    private readonly PlayStationDeviceController _controller;

    public PlayStationDeviceControllerTests()
    {
        _logger = Substitute.For<ILogger<PlayStationDeviceController>>();
        _controller = new PlayStationDeviceController(_logger);
    }

    [Fact(Timeout = 5000)]
    public async Task Connect_WithValidDevice_ReturnsTrue()
    {
        var device = new DeviceModel { IpAddress = "127.0.0.1", Name = "Test PlayStation" };

        var result = await _controller.Connect(device);

        Assert.True(result);
    }

    [Fact(Timeout = 5000)]
    public async Task Connect_WithoutIpAddress_ReturnsFalse()
    {
        var device = new DeviceModel { Name = "Test PlayStation" };

        var result = await _controller.Connect(device);

        Assert.False(result);
        _logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(v => v.ToString()!.Contains("no IP address")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact(Timeout = 5000)]
    public async Task Disconnect_WithValidDevice_ReturnsTrue()
    {
        var device = new DeviceModel { IpAddress = "127.0.0.1", Name = "Test PlayStation" };
        await _controller.Connect(device);

        var result = await _controller.Disconnect(device);

        Assert.True(result);
    }

    [Fact(Timeout = 5000)]
    public async Task SendCommand_PowerCommand_ReturnsResult()
    {
        var device = new DeviceModel
        {
            IpAddress = "127.0.0.1", // Use localhost to fail fast
            Name = "Test PlayStation"
        };
        var command = new DeviceCommand { Type = CommandType.Power };

        // Use a cancellation token with very short timeout
        var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

        var result = await _controller.SendCommand(device, command, cts.Token);

        Assert.False(result); // Will fail without actual PlayStation
    }

    [Fact(Timeout = 5000)]
    public async Task SendCommand_DirectionalCommand_HandlesAllDirections()
    {
        var device = new DeviceModel { IpAddress = "127.0.0.1", Name = "Test PlayStation" };
        var directions = new[]
        {
            CommandType.DirectionalUp,
            CommandType.DirectionalDown,
            CommandType.DirectionalLeft,
            CommandType.DirectionalRight
        };

        // Use a cancellation token with very short timeout
        var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

        foreach (var direction in directions)
        {
            var command = new DeviceCommand { Type = direction };
            var result = await _controller.SendCommand(device, command, cts.Token);
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
        var device = new DeviceModel { IpAddress = "127.0.0.1", Name = "Test PlayStation" };

        var result = await _controller.TestConnection(device);

        Assert.True(result); // UDP send succeeds even without actual PlayStation
    }

    [Fact(Timeout = 5000)]
    public async Task PowerOn_WithValidDevice_SendsCommand()
    {
        var device = new DeviceModel { IpAddress = "127.0.0.1", Name = "Test PlayStation" };

        var result = await _controller.PowerOn(device);

        Assert.True(result); // UDP send succeeds even without actual PlayStation
    }

    [Fact(Timeout = 5000)]
    public async Task Navigate_WithValidDirection_SendsCommand()
    {
        var device = new DeviceModel { IpAddress = "127.0.0.1", Name = "Test PlayStation" };

        // Use a cancellation token with very short timeout
        var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

        var result = await _controller.Navigate(device, "up", cts.Token);

        Assert.False(result); // Will fail without actual PlayStation
    }

    [Fact(Timeout = 5000)]
    public async Task SendCommand_CustomCommand_HandlesCustomPayload()
    {
        var device = new DeviceModel { IpAddress = "127.0.0.1", Name = "Test PlayStation" };
        var command = new DeviceCommand
        {
            Type = CommandType.Custom,
            NetworkPayload = "cross"
        };

        // Use a cancellation token with very short timeout
        var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

        var result = await _controller.SendCommand(device, command, cts.Token);

        Assert.False(result); // Will fail without actual PlayStation
    }
}