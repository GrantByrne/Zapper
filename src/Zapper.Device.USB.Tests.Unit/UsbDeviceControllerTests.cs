using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Zapper.Core.Models;

namespace Zapper.Device.USB.Tests.Unit;

public class UsbDeviceControllerTests
{
    private readonly IUsbRemoteHandler _mockRemoteHandler;
    private readonly ILogger<UsbDeviceController> _logger;
    private readonly UsbDeviceController _controller;

    public UsbDeviceControllerTests()
    {
        _mockRemoteHandler = Substitute.For<IUsbRemoteHandler>();
        _logger = NullLogger<UsbDeviceController>.Instance;
        _controller = new UsbDeviceController(_mockRemoteHandler, _logger);
    }

    [Fact]
    public async Task SendCommandAsync_WithUnsupportedDevice_ShouldReturnFalse()
    {
        var device = new Zapper.Core.Models.Device
        {
            Id = 1,
            Name = "Test Device",
            ConnectionType = ConnectionType.Bluetooth
        };
        var command = new DeviceCommand { Name = "Power" };

        var result = await _controller.SendCommandAsync(device, command);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task SendCommandAsync_WithUsbDevice_ShouldReturnFalse()
    {
        var device = new Zapper.Core.Models.Device
        {
            Id = 1,
            Name = "USB Remote",
            ConnectionType = ConnectionType.Usb
        };
        var command = new DeviceCommand { Name = "Power" };

        var result = await _controller.SendCommandAsync(device, command);

        // USB devices are input-only, so sending commands should return false
        result.Should().BeFalse();
    }

    [Fact]
    public async Task TestConnectionAsync_WithUnsupportedDevice_ShouldReturnFalse()
    {
        var device = new Zapper.Core.Models.Device
        {
            Id = 1,
            Name = "Test Device",
            ConnectionType = ConnectionType.Bluetooth
        };

        var result = await _controller.TestConnectionAsync(device);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task TestConnectionAsync_WithConnectedUsbDevice_ShouldReturnTrue()
    {
        var device = new Zapper.Core.Models.Device
        {
            Id = 1,
            Name = "USB Remote",
            ConnectionType = ConnectionType.Usb,
            MacAddress = "0001:0002:remote1"
        };

        _mockRemoteHandler.GetConnectedRemotes()
            .Returns(["USB:0001:0002:remote1", "USB:0003:0004:remote2"]);

        var result = await _controller.TestConnectionAsync(device);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task TestConnectionAsync_WithDisconnectedUsbDevice_ShouldReturnFalse()
    {
        var device = new Zapper.Core.Models.Device
        {
            Id = 1,
            Name = "USB Remote",
            ConnectionType = ConnectionType.Usb,
            MacAddress = "0001:0002:remote1"
        };

        _mockRemoteHandler.GetConnectedRemotes()
            .Returns(["USB:0003:0004:remote2"]);

        var result = await _controller.TestConnectionAsync(device);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task TestConnectionAsync_MatchingByName_ShouldReturnTrue()
    {
        var device = new Zapper.Core.Models.Device
        {
            Id = 1,
            Name = "LogitechRemote",
            ConnectionType = ConnectionType.Usb
        };

        _mockRemoteHandler.GetConnectedRemotes()
            .Returns(["USB:046D:C52B:LogitechRemote"]);

        var result = await _controller.TestConnectionAsync(device);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetStatusAsync_WithUnsupportedDevice_ShouldReturnOfflineStatus()
    {
        var device = new Zapper.Core.Models.Device
        {
            Id = 1,
            Name = "Test Device",
            ConnectionType = ConnectionType.Bluetooth
        };

        var result = await _controller.GetStatusAsync(device);

        result.IsOnline.Should().BeFalse();
        result.StatusMessage.Should().Be("Device not supported by USB controller");
    }

    [Fact]
    public async Task GetStatusAsync_WithConnectedUsbDevice_ShouldReturnOnlineStatus()
    {
        var device = new Zapper.Core.Models.Device
        {
            Id = 1,
            Name = "USB Remote",
            ConnectionType = ConnectionType.Usb,
            MacAddress = "0001:0002:remote1"
        };

        _mockRemoteHandler.GetConnectedRemotes()
            .Returns(["USB:0001:0002:remote1", "USB:0003:0004:remote2"]);
        _mockRemoteHandler.IsListening.Returns(true);

        var result = await _controller.GetStatusAsync(device);

        result.IsOnline.Should().BeTrue();
        result.StatusMessage.Should().Contain("USB remote connected");
        result.Properties.Should().NotBeNull();
        result.Properties!["IsListening"].Should().Be(true);
        result.Properties["ConnectedRemotes"].Should().Be(2);
        result.Properties.Should().ContainKey("LastSeen");
    }

    [Fact]
    public async Task GetStatusAsync_WithDisconnectedUsbDevice_ShouldReturnOfflineStatus()
    {
        var device = new Zapper.Core.Models.Device
        {
            Id = 1,
            Name = "USB Remote",
            ConnectionType = ConnectionType.Usb,
            MacAddress = "0001:0002:remote1"
        };

        _mockRemoteHandler.GetConnectedRemotes()
            .Returns(["USB:0003:0004:remote2"]);

        var result = await _controller.GetStatusAsync(device);

        result.IsOnline.Should().BeFalse();
        result.StatusMessage.Should().Be("USB remote not found");
    }

    [Theory]
    [InlineData(ConnectionType.Usb, true)]
    [InlineData(ConnectionType.Bluetooth, false)]
    [InlineData(ConnectionType.InfraredIr, false)]
    [InlineData(ConnectionType.NetworkTcp, false)]
    [InlineData(ConnectionType.WebOs, false)]
    public void SupportsDevice_ShouldReturnCorrectResult(ConnectionType connectionType, bool expectedResult)
    {
        var device = new Zapper.Core.Models.Device
        {
            Id = 1,
            Name = "Test Device",
            ConnectionType = connectionType
        };

        var result = _controller.SupportsDevice(device);

        result.Should().Be(expectedResult);
    }

    [Fact]
    public async Task TestConnectionAsync_WithEmptyConnectedRemotes_ShouldReturnFalse()
    {
        var device = new Zapper.Core.Models.Device
        {
            Id = 1,
            Name = "USB Remote",
            ConnectionType = ConnectionType.Usb,
            MacAddress = "0001:0002:remote1"
        };

        _mockRemoteHandler.GetConnectedRemotes()
            .Returns([]);

        var result = await _controller.TestConnectionAsync(device);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task TestConnectionAsync_WithNullAddress_ShouldStillMatchByName()
    {
        var device = new Zapper.Core.Models.Device
        {
            Id = 1,
            Name = "LogitechRemote",
            ConnectionType = ConnectionType.Usb,
            MacAddress = null
        };

        _mockRemoteHandler.GetConnectedRemotes()
            .Returns(["USB:046D:C52B:LogitechRemote"]);

        var result = await _controller.TestConnectionAsync(device);

        result.Should().BeTrue();
    }
}