using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Zapper.Core.Models;
using Zapper.Device.AppleTV.Controllers;
using Zapper.Device.AppleTV.Models;
using Zapper.Device.AppleTV.Tests.Unit.TestHelpers;

namespace Zapper.Device.AppleTV.Tests.Unit.Controllers;

public class BaseAppleTvControllerTests
{
    private readonly ILogger<BaseAppleTvController> _mockLogger;
    private readonly TestableBaseAppleTvController _controller;

    public BaseAppleTvControllerTests()
    {
        _mockLogger = Substitute.For<ILogger<BaseAppleTvController>>();
        _controller = new TestableBaseAppleTvController(_mockLogger);
    }

    [Fact]
    public async Task ConnectAsync_ValidDevice_ReturnsTrue()
    {
        // Arrange
        var device = new Zapper.Core.Models.Device
        {
            Id = 1,
            Name = "Test Apple TV",
            IpAddress = "192.168.1.100",
            Port = 7000
        };
        _controller.ShouldEstablishConnectionSucceed = true;

        // Act
        var result = await _controller.ConnectAsync(device);

        // Assert
        result.Should().BeTrue();
        _controller.EstablishConnectionAsyncCalled.Should().BeTrue();
        _controller.GetConnectedDevice().Should().Be(device);
        _controller.GetIsConnected().Should().BeTrue();
    }

    [Fact]
    public async Task ConnectAsync_EstablishConnectionFails_ReturnsFalse()
    {
        // Arrange
        var device = new Zapper.Core.Models.Device
        {
            Id = 1,
            Name = "Test Apple TV",
            IpAddress = "192.168.1.100"
        };
        _controller.ShouldEstablishConnectionSucceed = false;

        // Act
        var result = await _controller.ConnectAsync(device);

        // Assert
        result.Should().BeFalse();
        _controller.EstablishConnectionAsyncCalled.Should().BeTrue();
        _controller.GetIsConnected().Should().BeFalse();
    }

    [Fact]
    public async Task ConnectAsync_ExceptionThrown_ReturnsFalse()
    {
        // Arrange
        var controller = new ExceptionThrowingController(_mockLogger);
        var device = new Zapper.Core.Models.Device
        {
            Id = 1,
            Name = "Test Apple TV",
            IpAddress = "192.168.1.100"
        };

        // Act
        var result = await controller.ConnectAsync(device);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DisconnectAsync_WhenConnected_ReturnsTrue()
    {
        // Arrange
        _controller.SetIsConnected(true);

        // Act
        var result = await _controller.DisconnectAsync();

        // Assert
        result.Should().BeTrue();
        _controller.CloseConnectionAsyncCalled.Should().BeTrue();
        _controller.GetIsConnected().Should().BeFalse();
        _controller.GetConnectedDevice().Should().BeNull();
    }

    [Fact]
    public async Task DisconnectAsync_ExceptionThrown_ReturnsFalse()
    {
        // Arrange
        var controller = new ExceptionThrowingController(_mockLogger);
        controller.ShouldThrowOnClose = true;

        // Act
        var result = await controller.DisconnectAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsConnectedAsync_ReturnsCurrentState()
    {
        // Arrange & Act & Assert
        (await _controller.IsConnectedAsync()).Should().BeFalse();

        _controller.SetIsConnected(true);
        (await _controller.IsConnectedAsync()).Should().BeTrue();

        _controller.SetIsConnected(false);
        (await _controller.IsConnectedAsync()).Should().BeFalse();
    }

    [Theory]
    [InlineData("poweron", CommandCode.Wake)]
    [InlineData("power", CommandCode.Wake)]
    [InlineData("poweroff", CommandCode.Sleep)]
    [InlineData("volumeup", CommandCode.VolumeUp)]
    [InlineData("volumedown", CommandCode.VolumeDown)]
    [InlineData("mute", CommandCode.Mute)]
    [InlineData("menu", CommandCode.Menu)]
    [InlineData("home", CommandCode.Home)]
    [InlineData("back", CommandCode.Back)]
    [InlineData("select", CommandCode.Select)]
    [InlineData("ok", CommandCode.Select)]
    [InlineData("up", CommandCode.Up)]
    [InlineData("directionalup", CommandCode.Up)]
    [InlineData("down", CommandCode.Down)]
    [InlineData("directionaldown", CommandCode.Down)]
    [InlineData("left", CommandCode.Left)]
    [InlineData("directionalleft", CommandCode.Left)]
    [InlineData("right", CommandCode.Right)]
    [InlineData("directionalright", CommandCode.Right)]
    [InlineData("play", CommandCode.Play)]
    [InlineData("pause", CommandCode.Pause)]
    [InlineData("stop", CommandCode.Stop)]
    [InlineData("fastforward", CommandCode.FastForward)]
    [InlineData("forward", CommandCode.FastForward)]
    [InlineData("rewind", CommandCode.Rewind)]
    [InlineData("nexttrack", CommandCode.Next)]
    [InlineData("next", CommandCode.Next)]
    [InlineData("previoustrack", CommandCode.Previous)]
    [InlineData("previous", CommandCode.Previous)]
    public void MapDeviceCommand_ValidCommands_ReturnsCorrectCode(string commandName, CommandCode expectedCode)
    {
        // Arrange
        var command = new DeviceCommand { Name = commandName };

        // Act
        var result = _controller.TestMapDeviceCommand(command);

        // Assert
        result.Should().Be(expectedCode);
    }

    [Theory]
    [InlineData("unknown")]
    [InlineData("invalid")]
    [InlineData("")]
    public void MapDeviceCommand_UnknownCommands_ReturnsUnknown(string commandName)
    {
        // Arrange
        var command = new DeviceCommand { Name = commandName };

        // Act
        var result = _controller.TestMapDeviceCommand(command);

        // Assert
        result.Should().Be(CommandCode.Unknown);
    }

    [Fact]
    public void MapDeviceCommand_CaseInsensitive_ReturnsCorrectCode()
    {
        // Arrange
        var commands = new[]
        {
            new DeviceCommand { Name = "VOLUMEUP" },
            new DeviceCommand { Name = "VolumeUp" },
            new DeviceCommand { Name = "vOlUmEuP" }
        };

        // Act & Assert
        foreach (var command in commands)
        {
            _controller.TestMapDeviceCommand(command).Should().Be(CommandCode.VolumeUp);
        }
    }

    private class ExceptionThrowingController : TestableBaseAppleTvController
    {
        public bool ShouldThrowOnClose { get; set; }

        public ExceptionThrowingController(ILogger<BaseAppleTvController> logger) : base(logger)
        {
        }

        protected override Task<bool> EstablishConnectionAsync(Zapper.Core.Models.Device device)
        {
            throw new Exception("Test exception");
        }

        protected override Task CloseConnectionAsync()
        {
            if (ShouldThrowOnClose)
                throw new Exception("Test exception");
            return Task.CompletedTask;
        }
    }
}