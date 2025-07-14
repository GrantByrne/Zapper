using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Zapper.Core.Models;

namespace Zapper.Device.Bluetooth.Tests.Unit;

public class AndroidTvBluetoothControllerTests
{
    private readonly IBluetoothHidController _mockHidController;
    private readonly IBluetoothService _mockBluetoothService;
    private readonly ILogger<AndroidTvBluetoothController> _mockLogger;
    private readonly AndroidTvBluetoothController _controller;

    public AndroidTvBluetoothControllerTests()
    {
        _mockHidController = Substitute.For<IBluetoothHidController>();
        _mockBluetoothService = Substitute.For<IBluetoothService>();
        _mockLogger = Substitute.For<ILogger<AndroidTvBluetoothController>>();
        _controller = new AndroidTvBluetoothController(
            _mockHidController,
            _mockBluetoothService,
            _mockLogger);
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Act & Assert
        _controller.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullHidController_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => new AndroidTvBluetoothController(null!, _mockBluetoothService, _mockLogger);
        act.Should().Throw<ArgumentNullException>().WithParameterName("hidController");
    }

    [Fact]
    public void Constructor_WithNullBluetoothService_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => new AndroidTvBluetoothController(_mockHidController, null!, _mockLogger);
        act.Should().Throw<ArgumentNullException>().WithParameterName("bluetoothService");
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => new AndroidTvBluetoothController(_mockHidController, _mockBluetoothService, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Fact]
    public async Task SendCommandAsync_WithNonBluetoothDevice_ShouldReturnFalse()
    {
        // Arrange
        var device = new Zapper.Core.Models.Device
        {
            Id = 1,
            Name = "Test Device",
            ConnectionType = ConnectionType.NetworkTcp
        };
        var command = new DeviceCommand { Type = CommandType.Power };

        // Act
        var result = await _controller.SendCommandAsync(device, command);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SendCommandAsync_WithBluetoothDeviceButNoMacAddress_ShouldReturnFalse()
    {
        // Arrange
        var device = new Zapper.Core.Models.Device
        {
            Id = 1,
            Name = "Test Bluetooth Device",
            ConnectionType = ConnectionType.Bluetooth,
            MacAddress = null
        };
        var command = new DeviceCommand { Type = CommandType.Power };

        // Act
        var result = await _controller.SendCommandAsync(device, command);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SendCommandAsync_WithPowerCommand_ShouldSendHomeKey()
    {
        // Arrange
        var device = CreateTestBluetoothDevice();
        var command = new DeviceCommand { Type = CommandType.Power };

        _mockHidController.IsConnectedAsync(device.MacAddress!, Arg.Any<CancellationToken>())
            .Returns(true);
        _mockHidController.SendKeyAsync(device.MacAddress!, HidKeyCode.Home, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _controller.SendCommandAsync(device, command);

        // Assert
        result.Should().BeTrue();
        await _mockHidController.Received(1).SendKeyAsync(device.MacAddress!, HidKeyCode.Home, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendCommandAsync_WithVolumeUpCommand_ShouldSendVolumeUpKey()
    {
        // Arrange
        var device = CreateTestBluetoothDevice();
        var command = new DeviceCommand { Type = CommandType.VolumeUp };

        _mockHidController.IsConnectedAsync(device.MacAddress!, Arg.Any<CancellationToken>())
            .Returns(true);
        _mockHidController.SendKeyAsync(device.MacAddress!, HidKeyCode.VolumeUp, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _controller.SendCommandAsync(device, command);

        // Assert
        result.Should().BeTrue();
        await _mockHidController.Received(1).SendKeyAsync(device.MacAddress!, HidKeyCode.VolumeUp, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendCommandAsync_WithDirectionalUpCommand_ShouldSendDPadUpKey()
    {
        // Arrange
        var device = CreateTestBluetoothDevice();
        var command = new DeviceCommand { Type = CommandType.DirectionalUp };

        _mockHidController.IsConnectedAsync(device.MacAddress!, Arg.Any<CancellationToken>())
            .Returns(true);
        _mockHidController.SendKeyAsync(device.MacAddress!, HidKeyCode.DPadUp, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _controller.SendCommandAsync(device, command);

        // Assert
        result.Should().BeTrue();
        await _mockHidController.Received(1).SendKeyAsync(device.MacAddress!, HidKeyCode.DPadUp, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendCommandAsync_WithNumberCommand_ShouldSendCorrectNumberKey()
    {
        // Arrange
        var device = CreateTestBluetoothDevice();
        var command = new DeviceCommand 
        { 
            Type = CommandType.Number,
            NetworkPayload = "5"
        };

        _mockHidController.IsConnectedAsync(device.MacAddress!, Arg.Any<CancellationToken>())
            .Returns(true);
        _mockHidController.SendKeyAsync(device.MacAddress!, HidKeyCode.Key5, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _controller.SendCommandAsync(device, command);

        // Assert
        result.Should().BeTrue();
        await _mockHidController.Received(1).SendKeyAsync(device.MacAddress!, HidKeyCode.Key5, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendCommandAsync_WithCustomTextCommand_ShouldSendText()
    {
        // Arrange
        var device = CreateTestBluetoothDevice();
        var command = new DeviceCommand 
        { 
            Type = CommandType.Custom,
            NetworkPayload = "text:Hello World"
        };

        _mockHidController.IsConnectedAsync(device.MacAddress!, Arg.Any<CancellationToken>())
            .Returns(true);
        _mockHidController.SendTextAsync(device.MacAddress!, "Hello World", Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _controller.SendCommandAsync(device, command);

        // Assert
        result.Should().BeTrue();
        await _mockHidController.Received(1).SendTextAsync(device.MacAddress!, "Hello World", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendCommandAsync_WithCustomNetflixCommand_ShouldSendKeySequence()
    {
        // Arrange
        var device = CreateTestBluetoothDevice();
        var command = new DeviceCommand 
        { 
            Type = CommandType.Custom,
            NetworkPayload = "netflix"
        };

        _mockHidController.IsConnectedAsync(device.MacAddress!, Arg.Any<CancellationToken>())
            .Returns(true);
        _mockHidController.SendKeySequenceAsync(
            device.MacAddress!, 
            Arg.Is<HidKeyCode[]>(keys => keys.SequenceEqual(new[] { HidKeyCode.Home, HidKeyCode.N })),
            100,
            Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _controller.SendCommandAsync(device, command);

        // Assert
        result.Should().BeTrue();
        await _mockHidController.Received(1).SendKeySequenceAsync(
            device.MacAddress!, 
            Arg.Is<HidKeyCode[]>(keys => keys.SequenceEqual(new[] { HidKeyCode.Home, HidKeyCode.N })),
            100,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendCommandAsync_WithDisconnectedDevice_ShouldAttemptConnection()
    {
        // Arrange
        var device = CreateTestBluetoothDevice();
        var command = new DeviceCommand { Type = CommandType.Home };

        _mockHidController.IsConnectedAsync(device.MacAddress!, Arg.Any<CancellationToken>())
            .Returns(false);
        _mockHidController.ConnectAsync(device.MacAddress!, Arg.Any<CancellationToken>())
            .Returns(true);
        _mockHidController.SendKeyAsync(device.MacAddress!, HidKeyCode.Home, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _controller.SendCommandAsync(device, command);

        // Assert
        result.Should().BeTrue();
        await _mockHidController.Received(1).ConnectAsync(device.MacAddress!, Arg.Any<CancellationToken>());
        await _mockHidController.Received(1).SendKeyAsync(device.MacAddress!, HidKeyCode.Home, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendCommandAsync_WithConnectionFailure_ShouldReturnFalse()
    {
        // Arrange
        var device = CreateTestBluetoothDevice();
        var command = new DeviceCommand { Type = CommandType.Home };

        _mockHidController.IsConnectedAsync(device.MacAddress!, Arg.Any<CancellationToken>())
            .Returns(false);
        _mockHidController.ConnectAsync(device.MacAddress!, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _controller.SendCommandAsync(device, command);

        // Assert
        result.Should().BeFalse();
        await _mockHidController.Received(1).ConnectAsync(device.MacAddress!, Arg.Any<CancellationToken>());
        await _mockHidController.DidNotReceive().SendKeyAsync(Arg.Any<string>(), Arg.Any<HidKeyCode>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TestConnectionAsync_WithValidBluetoothDevice_ShouldCallBluetoothService()
    {
        // Arrange
        var device = CreateTestBluetoothDevice();
        var deviceInfo = new BluetoothDeviceInfo
        {
            Address = device.MacAddress!,
            Name = device.Name,
            IsConnected = true
        };

        _mockBluetoothService.GetDeviceAsync(device.MacAddress!, Arg.Any<CancellationToken>())
            .Returns(deviceInfo);

        // Act
        var result = await _controller.TestConnectionAsync(device);

        // Assert
        result.Should().BeTrue();
        await _mockBluetoothService.Received(1).GetDeviceAsync(device.MacAddress!, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TestConnectionAsync_WithNonBluetoothDevice_ShouldReturnFalse()
    {
        // Arrange
        var device = new Zapper.Core.Models.Device
        {
            Id = 1,
            Name = "Test Device",
            ConnectionType = ConnectionType.NetworkTcp
        };

        // Act
        var result = await _controller.TestConnectionAsync(device);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DiscoverPairedDevicesAsync_ShouldReturnOnlyAndroidTVDevices()
    {
        // Arrange
        var devices = new[]
        {
            new BluetoothDeviceInfo
            {
                Address = "AA:BB:CC:DD:EE:FF",
                Name = "Android TV Remote",
                IsPaired = true
            },
            new BluetoothDeviceInfo
            {
                Address = "11:22:33:44:55:66",
                Name = "Chromecast Audio",
                IsPaired = true
            },
            new BluetoothDeviceInfo
            {
                Address = "99:88:77:66:55:44",
                Name = "Random Device",
                IsPaired = true
            }
        };

        _mockBluetoothService.GetDevicesAsync(Arg.Any<CancellationToken>())
            .Returns(devices);

        // Act
        var result = await _controller.DiscoverPairedDevicesAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain("AA:BB:CC:DD:EE:FF");
        result.Should().Contain("11:22:33:44:55:66");
        result.Should().NotContain("99:88:77:66:55:44");
    }

    private static Zapper.Core.Models.Device CreateTestBluetoothDevice()
    {
        return new Zapper.Core.Models.Device
        {
            Id = 1,
            Name = "Test Android TV",
            ConnectionType = ConnectionType.Bluetooth,
            MacAddress = "AA:BB:CC:DD:EE:FF"
        };
    }
}