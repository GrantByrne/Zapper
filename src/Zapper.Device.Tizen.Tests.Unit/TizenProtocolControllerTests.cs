using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Zapper.Core.Models;
using Xunit;

namespace Zapper.Device.Tizen.Tests.Unit;

public class TizenProtocolControllerTests
{
    private readonly ITizenDeviceController _mockTizenController;
    private readonly ILogger<TizenProtocolController> _mockLogger;
    private readonly TizenProtocolController _controller;

    public TizenProtocolControllerTests()
    {
        _mockTizenController = Substitute.For<ITizenDeviceController>();
        _mockLogger = Substitute.For<ILogger<TizenProtocolController>>();
        _controller = new TizenProtocolController(_mockTizenController, _mockLogger);
    }

    [Fact]
    public async Task SendCommand_WithNonTizenDevice_ShouldReturnFalse()
    {
        var device = new Zapper.Core.Models.Device
        {
            Id = 1,
            Name = "Test Device",
            ConnectionType = ConnectionType.InfraredIr
        };
        var command = new DeviceCommand { Name = "Power", Type = CommandType.Power };

        var result = await _controller.SendCommand(device, command);

        result.Should().BeFalse();
        _mockLogger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("is not supported by Tizen controller")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task SendCommand_WithTizenDevice_ShouldCallTizenController()
    {
        var device = new Zapper.Core.Models.Device
        {
            Name = "Samsung TV",
            ConnectionType = ConnectionType.Tizen
        };
        var command = new DeviceCommand { Name = "Power", Type = CommandType.Power };

        _mockTizenController.SendCommand(device, command, Arg.Any<CancellationToken>()).Returns(true);

        var result = await _controller.SendCommand(device, command);

        result.Should().BeTrue();
        await _mockTizenController.Received(1).SendCommand(device, command, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendCommand_WithDelay_ShouldWaitAfterCommand()
    {
        var device = new Zapper.Core.Models.Device
        {
            Name = "Samsung TV",
            ConnectionType = ConnectionType.Tizen
        };
        var command = new DeviceCommand 
        { 
            Name = "Power", 
            Type = CommandType.Power,
            DelayMs = 100
        };

        _mockTizenController.SendCommand(device, command, Arg.Any<CancellationToken>()).Returns(true);

        var startTime = DateTime.UtcNow;
        var result = await _controller.SendCommand(device, command);
        var elapsed = DateTime.UtcNow - startTime;

        result.Should().BeTrue();
        elapsed.TotalMilliseconds.Should().BeGreaterThanOrEqualTo(90); // Allow some tolerance
        _mockLogger.Received(1).Log(
            LogLevel.Debug,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Successfully sent Tizen command")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task SendCommand_WhenTizenControllerThrows_ShouldLogErrorAndReturnFalse()
    {
        var device = new Zapper.Core.Models.Device
        {
            Name = "Samsung TV",
            ConnectionType = ConnectionType.Tizen
        };
        var command = new DeviceCommand { Name = "Power", Type = CommandType.Power };

        _mockTizenController.SendCommand(device, command, Arg.Any<CancellationToken>())
            .Returns(Task.FromException<bool>(new Exception("Test exception")));

        var result = await _controller.SendCommand(device, command);

        result.Should().BeFalse();
        _mockLogger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Failed to send Tizen command")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task TestConnection_WithNonTizenDevice_ShouldReturnFalse()
    {
        var device = new Zapper.Core.Models.Device
        {
            ConnectionType = ConnectionType.InfraredIr
        };

        var result = await _controller.TestConnection(device);

        result.Should().BeFalse();
        await _mockTizenController.DidNotReceive().TestConnection(Arg.Any<Zapper.Core.Models.Device>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TestConnection_WithTizenDevice_ShouldCallTizenController()
    {
        var device = new Zapper.Core.Models.Device
        {
            Name = "Samsung TV",
            ConnectionType = ConnectionType.Tizen
        };

        _mockTizenController.TestConnection(device, Arg.Any<CancellationToken>()).Returns(true);

        var result = await _controller.TestConnection(device);

        result.Should().BeTrue();
        await _mockTizenController.Received(1).TestConnection(device, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TestConnection_WhenTizenControllerThrows_ShouldLogErrorAndReturnFalse()
    {
        var device = new Zapper.Core.Models.Device
        {
            Name = "Samsung TV",
            ConnectionType = ConnectionType.Tizen
        };

        _mockTizenController.TestConnection(device, Arg.Any<CancellationToken>())
            .Returns(Task.FromException<bool>(new Exception("Test exception")));

        var result = await _controller.TestConnection(device);

        result.Should().BeFalse();
        _mockLogger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Failed to test Tizen connection")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task GetStatus_WithTizenDevice_WhenOnline_ShouldReturnOnlineStatus()
    {
        var device = new Zapper.Core.Models.Device
        {
            Name = "Samsung TV",
            ConnectionType = ConnectionType.Tizen
        };

        _mockTizenController.TestConnection(device, Arg.Any<CancellationToken>()).Returns(true);

        var result = await _controller.GetStatus(device);

        result.Should().NotBeNull();
        result.IsOnline.Should().BeTrue();
        result.StatusMessage.Should().Be("Samsung Tizen TV connected");
    }

    [Fact]
    public async Task GetStatus_WithTizenDevice_WhenOffline_ShouldReturnOfflineStatus()
    {
        var device = new Zapper.Core.Models.Device
        {
            Name = "Samsung TV",
            ConnectionType = ConnectionType.Tizen
        };

        _mockTizenController.TestConnection(device, Arg.Any<CancellationToken>()).Returns(false);

        var result = await _controller.GetStatus(device);

        result.Should().NotBeNull();
        result.IsOnline.Should().BeFalse();
        result.StatusMessage.Should().Be("Samsung Tizen TV not reachable");
    }

    [Fact]
    public async Task GetStatus_WhenTestConnectionThrows_ShouldReturnErrorStatus()
    {
        var device = new Zapper.Core.Models.Device
        {
            Name = "Samsung TV",
            ConnectionType = ConnectionType.Tizen
        };
        var exception = new Exception("Connection failed");

        _mockTizenController.TestConnection(device, Arg.Any<CancellationToken>())
            .Returns(Task.FromException<bool>(exception));

        var result = await _controller.GetStatus(device);

        result.Should().NotBeNull();
        result.IsOnline.Should().BeFalse();
        result.StatusMessage.Should().Be("Samsung Tizen TV not reachable");
        
        // The exception is caught in TestConnection, which logs it and returns false
        // So GetStatus won't log an error, it will just see false result
        _mockLogger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Failed to test Tizen connection")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Theory]
    [InlineData(ConnectionType.Tizen, true)]
    [InlineData(ConnectionType.InfraredIr, false)]
    [InlineData(ConnectionType.NetworkHttp, false)]
    [InlineData(ConnectionType.WebOs, false)]
    public void SupportsDevice_ShouldReturnCorrectValue(ConnectionType connectionType, bool expected)
    {
        var device = new Zapper.Core.Models.Device
        {
            ConnectionType = connectionType
        };

        var result = _controller.SupportsDevice(device);

        result.Should().Be(expected);
    }
}