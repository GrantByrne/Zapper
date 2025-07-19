using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Zapper.Core.Models;

namespace Zapper.Device.AndroidTV.Tests.Unit;

public class AndroidTvAdbControllerTests
{
    private readonly IAdbClient _mockAdbClient;
    private readonly ILogger<AndroidTvAdbController> _mockLogger;
    private readonly AndroidTvAdbController _controller;

    public AndroidTvAdbControllerTests()
    {
        _mockAdbClient = Substitute.For<IAdbClient>();
        _mockLogger = Substitute.For<ILogger<AndroidTvAdbController>>();
        _controller = new AndroidTvAdbController(_mockAdbClient, _mockLogger);
    }

    [Fact]
    public async Task SendCommand_DeviceNotSupported_ReturnsFalse()
    {
        // Arrange
        var device = new Zapper.Core.Models.Device
        {
            Type = DeviceType.Television,
            ConnectionType = ConnectionType.InfraredIr,
            IpAddress = "192.168.1.100"
        };
        var command = new DeviceCommand { Type = CommandType.Power };

        // Act
        var result = await _controller.SendCommand(device, command);

        // Assert
        result.Should().BeFalse();
        await _mockAdbClient.DidNotReceive().ConnectAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendCommand_NoIpAddress_ReturnsFalse()
    {
        // Arrange
        var device = new Zapper.Core.Models.Device
        {
            Type = DeviceType.AndroidTv,
            ConnectionType = ConnectionType.Adb,
            IpAddress = null
        };
        var command = new DeviceCommand { Type = CommandType.Power };

        // Act
        var result = await _controller.SendCommand(device, command);

        // Assert
        result.Should().BeFalse();
        await _mockAdbClient.DidNotReceive().ConnectAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendCommand_NotConnected_ConnectsAndExecutesCommand()
    {
        // Arrange
        var device = new Zapper.Core.Models.Device
        {
            Type = DeviceType.AndroidTv,
            ConnectionType = ConnectionType.Adb,
            IpAddress = "192.168.1.100",
            Port = 5555
        };
        var command = new DeviceCommand { Type = CommandType.Power };

        _mockAdbClient.IsConnected.Returns(false);
        _mockAdbClient.ConnectAsync(device.IpAddress, device.Port.Value, Arg.Any<CancellationToken>()).Returns(true);
        _mockAdbClient.ExecuteShellCommandAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);

        // Act
        var result = await _controller.SendCommand(device, command);

        // Assert
        result.Should().BeTrue();
        await _mockAdbClient.Received(1).ConnectAsync(device.IpAddress, device.Port.Value, Arg.Any<CancellationToken>());
        await _mockAdbClient.Received(1).ExecuteShellCommandAsync($"input keyevent {AndroidKeyEvents.Power}", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendCommand_AlreadyConnected_DoesNotReconnect()
    {
        // Arrange
        var device = new Zapper.Core.Models.Device
        {
            Type = DeviceType.AndroidTv,
            ConnectionType = ConnectionType.Adb,
            IpAddress = "192.168.1.100"
        };
        var command = new DeviceCommand { Type = CommandType.VolumeUp };

        _mockAdbClient.IsConnected.Returns(true);
        _mockAdbClient.ExecuteShellCommandAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);

        // Act
        var result = await _controller.SendCommand(device, command);

        // Assert
        result.Should().BeTrue();
        await _mockAdbClient.DidNotReceive().ConnectAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        await _mockAdbClient.Received(1).ExecuteShellCommandAsync($"input keyevent {AndroidKeyEvents.VolumeUp}", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendCommand_ConnectionFails_ReturnsFalse()
    {
        // Arrange
        var device = new Zapper.Core.Models.Device
        {
            Type = DeviceType.AndroidTv,
            ConnectionType = ConnectionType.Adb,
            IpAddress = "192.168.1.100"
        };
        var command = new DeviceCommand { Type = CommandType.Power };

        _mockAdbClient.IsConnected.Returns(false);
        _mockAdbClient.ConnectAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(false);

        // Act
        var result = await _controller.SendCommand(device, command);

        // Assert
        result.Should().BeFalse();
        await _mockAdbClient.Received(1).ConnectAsync(device.IpAddress, 5555, Arg.Any<CancellationToken>());
        await _mockAdbClient.DidNotReceive().ExecuteShellCommandAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(CommandType.Power, AndroidKeyEvents.Power)]
    [InlineData(CommandType.VolumeUp, AndroidKeyEvents.VolumeUp)]
    [InlineData(CommandType.VolumeDown, AndroidKeyEvents.VolumeDown)]
    [InlineData(CommandType.Mute, AndroidKeyEvents.VolumeMute)]
    [InlineData(CommandType.ChannelUp, AndroidKeyEvents.ChannelUp)]
    [InlineData(CommandType.ChannelDown, AndroidKeyEvents.ChannelDown)]
    [InlineData(CommandType.Menu, AndroidKeyEvents.Menu)]
    [InlineData(CommandType.Back, AndroidKeyEvents.Back)]
    [InlineData(CommandType.Home, AndroidKeyEvents.Home)]
    [InlineData(CommandType.DirectionalUp, AndroidKeyEvents.DpadUp)]
    [InlineData(CommandType.DirectionalDown, AndroidKeyEvents.DpadDown)]
    [InlineData(CommandType.DirectionalLeft, AndroidKeyEvents.DpadLeft)]
    [InlineData(CommandType.DirectionalRight, AndroidKeyEvents.DpadRight)]
    [InlineData(CommandType.Ok, AndroidKeyEvents.DpadCenter)]
    [InlineData(CommandType.PlayPause, AndroidKeyEvents.MediaPlayPause)]
    [InlineData(CommandType.Stop, AndroidKeyEvents.MediaStop)]
    [InlineData(CommandType.FastForward, AndroidKeyEvents.MediaFastForward)]
    [InlineData(CommandType.Rewind, AndroidKeyEvents.MediaRewind)]
    public async Task SendCommand_ValidCommands_SendsCorrectKeyEvent(CommandType commandType, int expectedKeyEvent)
    {
        // Arrange
        var device = new Zapper.Core.Models.Device
        {
            Type = DeviceType.AndroidTv,
            ConnectionType = ConnectionType.Adb,
            IpAddress = "192.168.1.100"
        };
        var command = new DeviceCommand { Type = commandType };

        _mockAdbClient.IsConnected.Returns(true);
        _mockAdbClient.ExecuteShellCommandAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);

        // Act
        var result = await _controller.SendCommand(device, command);

        // Assert
        result.Should().BeTrue();
        await _mockAdbClient.Received(1).ExecuteShellCommandAsync($"input keyevent {expectedKeyEvent}", Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("0", AndroidKeyEvents.Keycode0)]
    [InlineData("1", AndroidKeyEvents.Keycode1)]
    [InlineData("2", AndroidKeyEvents.Keycode2)]
    [InlineData("3", AndroidKeyEvents.Keycode3)]
    [InlineData("4", AndroidKeyEvents.Keycode4)]
    [InlineData("5", AndroidKeyEvents.Keycode5)]
    [InlineData("6", AndroidKeyEvents.Keycode6)]
    [InlineData("7", AndroidKeyEvents.Keycode7)]
    [InlineData("8", AndroidKeyEvents.Keycode8)]
    [InlineData("9", AndroidKeyEvents.Keycode9)]
    public async Task SendCommand_NumberCommands_SendsCorrectKeyEvent(string numberPayload, int expectedKeyEvent)
    {
        // Arrange
        var device = new Zapper.Core.Models.Device
        {
            Type = DeviceType.AndroidTv,
            ConnectionType = ConnectionType.Adb,
            IpAddress = "192.168.1.100"
        };
        var command = new DeviceCommand 
        { 
            Type = CommandType.Number,
            NetworkPayload = numberPayload
        };

        _mockAdbClient.IsConnected.Returns(true);
        _mockAdbClient.ExecuteShellCommandAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);

        // Act
        var result = await _controller.SendCommand(device, command);

        // Assert
        result.Should().BeTrue();
        await _mockAdbClient.Received(1).ExecuteShellCommandAsync($"input keyevent {expectedKeyEvent}", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendCommand_KeyboardInput_SendsTextInput()
    {
        // Arrange
        var device = new Zapper.Core.Models.Device
        {
            Type = DeviceType.AndroidTv,
            ConnectionType = ConnectionType.Adb,
            IpAddress = "192.168.1.100"
        };
        var command = new DeviceCommand 
        { 
            Type = CommandType.KeyboardInput,
            KeyboardText = "Hello World"
        };

        _mockAdbClient.IsConnected.Returns(true);
        _mockAdbClient.ExecuteShellCommandAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);

        // Act
        var result = await _controller.SendCommand(device, command);

        // Assert
        result.Should().BeTrue();
        await _mockAdbClient.Received(1).ExecuteShellCommandAsync("input text 'Hello%sWorld'", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendCommand_AppLaunch_SendsAppIntent()
    {
        // Arrange
        var device = new Zapper.Core.Models.Device
        {
            Type = DeviceType.AndroidTv,
            ConnectionType = ConnectionType.Adb,
            IpAddress = "192.168.1.100"
        };
        var command = new DeviceCommand 
        { 
            Type = CommandType.AppLaunch,
            NetworkPayload = "com.netflix.ninja"
        };

        _mockAdbClient.IsConnected.Returns(true);
        _mockAdbClient.ExecuteShellCommandAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);

        // Act
        var result = await _controller.SendCommand(device, command);

        // Assert
        result.Should().BeTrue();
        await _mockAdbClient.Received(1).ExecuteShellCommandAsync("am start -n com.netflix.ninja", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendCommand_UnsupportedCommand_ReturnsFalse()
    {
        // Arrange
        var device = new Zapper.Core.Models.Device
        {
            Type = DeviceType.AndroidTv,
            ConnectionType = ConnectionType.Adb,
            IpAddress = "192.168.1.100"
        };
        var command = new DeviceCommand { Type = (CommandType)999 }; // Unknown command type

        _mockAdbClient.IsConnected.Returns(true);

        // Act
        var result = await _controller.SendCommand(device, command);

        // Assert
        result.Should().BeFalse();
        await _mockAdbClient.DidNotReceive().ExecuteShellCommandAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TestConnection_ValidDevice_ReturnsTrue()
    {
        // Arrange
        var device = new Zapper.Core.Models.Device
        {
            Type = DeviceType.AndroidTv,
            ConnectionType = ConnectionType.Adb,
            IpAddress = "192.168.1.100",
            Port = 5555
        };

        _mockAdbClient.ConnectAsync(device.IpAddress, device.Port.Value, Arg.Any<CancellationToken>()).Returns(true);
        _mockAdbClient.ExecuteShellCommandWithResponseAsync("echo 'test'", Arg.Any<CancellationToken>()).Returns("test");

        // Act
        var result = await _controller.TestConnection(device);

        // Assert
        result.Should().BeTrue();
        await _mockAdbClient.Received(1).ConnectAsync(device.IpAddress, device.Port.Value, Arg.Any<CancellationToken>());
        await _mockAdbClient.Received(1).ExecuteShellCommandWithResponseAsync("echo 'test'", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TestConnection_ConnectionFails_ReturnsFalse()
    {
        // Arrange
        var device = new Zapper.Core.Models.Device
        {
            Type = DeviceType.AndroidTv,
            ConnectionType = ConnectionType.Adb,
            IpAddress = "192.168.1.100"
        };

        _mockAdbClient.ConnectAsync(device.IpAddress, 5555, Arg.Any<CancellationToken>()).Returns(false);

        // Act
        var result = await _controller.TestConnection(device);

        // Assert
        result.Should().BeFalse();
        await _mockAdbClient.Received(1).ConnectAsync(device.IpAddress, 5555, Arg.Any<CancellationToken>());
    }

    [Fact]
    public void SupportsDevice_AndroidTvWithAdb_ReturnsTrue()
    {
        // Arrange
        var device = new Zapper.Core.Models.Device
        {
            Type = DeviceType.AndroidTv,
            ConnectionType = ConnectionType.Adb,
            IpAddress = "192.168.1.100"
        };

        // Act
        var result = _controller.SupportsDevice(device);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void SupportsDevice_NonAndroidTvDevice_ReturnsFalse()
    {
        // Arrange
        var device = new Zapper.Core.Models.Device
        {
            Type = DeviceType.Television,
            ConnectionType = ConnectionType.Adb
        };

        // Act
        var result = _controller.SupportsDevice(device);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void SupportsDevice_AndroidTvWithNonAdbConnection_ReturnsFalse()
    {
        // Arrange
        var device = new Zapper.Core.Models.Device
        {
            Type = DeviceType.AndroidTv,
            ConnectionType = ConnectionType.InfraredIr
        };

        // Act
        var result = _controller.SupportsDevice(device);

        // Assert
        result.Should().BeFalse();
    }
}