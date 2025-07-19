using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Zapper.Core.Models;
using Xunit;

namespace Zapper.Device.Tizen.Tests.Unit;

public class TizenHardwareControllerTests
{
    private readonly ITizenClient _mockTizenClient;
    private readonly ILogger<TizenHardwareController> _mockLogger;
    private readonly TizenHardwareController _controller;

    public TizenHardwareControllerTests()
    {
        _mockTizenClient = Substitute.For<ITizenClient>();
        _mockLogger = Substitute.For<ILogger<TizenHardwareController>>();
        _controller = new TizenHardwareController(_mockTizenClient, _mockLogger);
    }

    [Fact]
    public async Task SendCommand_WithNonTizenDevice_ShouldReturnFalse()
    {
        var device = new Zapper.Core.Models.Device
        {
            Name = "Test Device",
            ConnectionType = ConnectionType.InfraredIr,
            NetworkAddress = "192.168.1.100"
        };
        var command = new DeviceCommand { Name = "Power", Type = CommandType.Power };

        var result = await _controller.SendCommand(device, command);

        result.Should().BeFalse();
        _mockLogger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("is not a Tizen device")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task SendCommand_WithNoNetworkAddress_ShouldReturnFalse()
    {
        var device = new Zapper.Core.Models.Device
        {
            Name = "Test Device",
            ConnectionType = ConnectionType.Tizen,
            NetworkAddress = null
        };
        var command = new DeviceCommand { Name = "Power", Type = CommandType.Power };

        var result = await _controller.SendCommand(device, command);

        result.Should().BeFalse();
        _mockLogger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("has no network address configured")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task SendCommand_WhenNotConnected_ShouldAttemptConnection()
    {
        var device = new Zapper.Core.Models.Device
        {
            Name = "Samsung TV",
            ConnectionType = ConnectionType.Tizen,
            NetworkAddress = "192.168.1.100",
            AuthenticationToken = "test-token"
        };
        var command = new DeviceCommand { Name = "Power", Type = CommandType.Power };

        _mockTizenClient.IsConnected.Returns(false);
        _mockTizenClient.ConnectAsync(device.NetworkAddress, device.AuthenticationToken, Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await _controller.SendCommand(device, command);

        result.Should().BeFalse();
        await _mockTizenClient.Received(1).ConnectAsync(device.NetworkAddress, device.AuthenticationToken, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendCommand_PowerCommand_ShouldCallPowerOffAsync()
    {
        var device = CreateValidDevice();
        var command = new DeviceCommand { Name = "Power", Type = CommandType.Power };

        _mockTizenClient.IsConnected.Returns(true);
        _mockTizenClient.PowerOffAsync(Arg.Any<CancellationToken>()).Returns(true);

        var result = await _controller.SendCommand(device, command);

        result.Should().BeTrue();
        await _mockTizenClient.Received(1).PowerOffAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendCommand_VolumeUpCommand_ShouldCallVolumeUpAsync()
    {
        var device = CreateValidDevice();
        var command = new DeviceCommand { Name = "VolumeUp", Type = CommandType.VolumeUp };

        _mockTizenClient.IsConnected.Returns(true);
        _mockTizenClient.VolumeUpAsync(Arg.Any<CancellationToken>()).Returns(true);

        var result = await _controller.SendCommand(device, command);

        result.Should().BeTrue();
        await _mockTizenClient.Received(1).VolumeUpAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendCommand_DirectionalCommands_ShouldCallSendKeyWithCorrectKeys()
    {
        var device = CreateValidDevice();
        var testCases = new[]
        {
            (CommandType.DirectionalUp, "KEY_UP"),
            (CommandType.DirectionalDown, "KEY_DOWN"),
            (CommandType.DirectionalLeft, "KEY_LEFT"),
            (CommandType.DirectionalRight, "KEY_RIGHT"),
            (CommandType.Ok, "KEY_ENTER"),
            (CommandType.Back, "KEY_RETURN"),
            (CommandType.Home, "KEY_HOME"),
            (CommandType.Menu, "KEY_MENU")
        };

        _mockTizenClient.IsConnected.Returns(true);
        _mockTizenClient.SendKeyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);

        foreach (var (commandType, expectedKey) in testCases)
        {
            var command = new DeviceCommand { Name = commandType.ToString(), Type = commandType };
            
            var result = await _controller.SendCommand(device, command);

            result.Should().BeTrue();
            await _mockTizenClient.Received(1).SendKeyAsync(expectedKey, Arg.Any<CancellationToken>());
            _mockTizenClient.ClearReceivedCalls();
        }
    }

    [Fact]
    public async Task SendCommand_AppLaunchCommand_WithoutPayload_ShouldReturnFalse()
    {
        var device = CreateValidDevice();
        var command = new DeviceCommand { Name = "LaunchApp", Type = CommandType.AppLaunch, NetworkPayload = null };

        _mockTizenClient.IsConnected.Returns(true);

        var result = await _controller.SendCommand(device, command);

        result.Should().BeFalse();
        _mockLogger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("App ID parameter is required")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task SendCommand_AppLaunchCommand_WithPayload_ShouldCallLaunchAppAsync()
    {
        var device = CreateValidDevice();
        var appId = "com.netflix.app";
        var command = new DeviceCommand { Name = "LaunchApp", Type = CommandType.AppLaunch, NetworkPayload = appId };

        _mockTizenClient.IsConnected.Returns(true);
        _mockTizenClient.LaunchAppAsync(appId, Arg.Any<CancellationToken>()).Returns(true);

        var result = await _controller.SendCommand(device, command);

        result.Should().BeTrue();
        await _mockTizenClient.Received(1).LaunchAppAsync(appId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendCommand_KeyboardInputCommand_ShouldCallSendTextAsync()
    {
        var device = CreateValidDevice();
        var text = "Hello World";
        var command = new DeviceCommand { Name = "KeyboardInput", Type = CommandType.KeyboardInput, NetworkPayload = text };

        _mockTizenClient.IsConnected.Returns(true);
        _mockTizenClient.SendTextAsync(text, Arg.Any<CancellationToken>()).Returns(true);

        var result = await _controller.SendCommand(device, command);

        result.Should().BeTrue();
        await _mockTizenClient.Received(1).SendTextAsync(text, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendCommand_CustomCommand_WithIrCode_ShouldCallSendKeyAsync()
    {
        var device = CreateValidDevice();
        var irCode = "KEY_CUSTOM";
        var command = new DeviceCommand { Name = "Custom", Type = CommandType.Custom, IrCode = irCode };

        _mockTizenClient.IsConnected.Returns(true);
        _mockTizenClient.SendKeyAsync(irCode, Arg.Any<CancellationToken>()).Returns(true);

        var result = await _controller.SendCommand(device, command);

        result.Should().BeTrue();
        await _mockTizenClient.Received(1).SendKeyAsync(irCode, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendCommand_WithException_ShouldLogErrorAndReturnFalse()
    {
        var device = CreateValidDevice();
        var command = new DeviceCommand { Name = "Power", Type = CommandType.Power };

        _mockTizenClient.IsConnected.Returns(true);
        _mockTizenClient.PowerOffAsync(Arg.Any<CancellationToken>())
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
    public async Task TestConnection_ShouldSendInfoCommand()
    {
        var device = CreateValidDevice();

        _mockTizenClient.IsConnected.Returns(true);
        _mockTizenClient.SendKeyAsync("KEY_INFO", Arg.Any<CancellationToken>()).Returns(true);

        var result = await _controller.TestConnection(device);

        result.Should().BeTrue();
        await _mockTizenClient.Received(1).SendKeyAsync("KEY_INFO", Arg.Any<CancellationToken>());
    }

    private static Zapper.Core.Models.Device CreateValidDevice()
    {
        return new Zapper.Core.Models.Device
        {
            Name = "Samsung TV",
            ConnectionType = ConnectionType.Tizen,
            NetworkAddress = "192.168.1.100",
            AuthenticationToken = "test-token"
        };
    }
}