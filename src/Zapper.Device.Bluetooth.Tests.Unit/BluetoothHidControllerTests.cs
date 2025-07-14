using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Zapper.Device.Bluetooth.Tests.Unit;

public class BluetoothHidControllerTests
{
    private readonly IBluetoothService _mockBluetoothService;
    private readonly ILogger<BluetoothHidController> _mockLogger;
    private readonly BluetoothHidController _hidController;

    public BluetoothHidControllerTests()
    {
        _mockBluetoothService = Substitute.For<IBluetoothService>();
        _mockLogger = Substitute.For<ILogger<BluetoothHidController>>();
        _hidController = new BluetoothHidController(_mockBluetoothService, _mockLogger);
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Act & Assert
        _hidController.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullBluetoothService_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => new BluetoothHidController(null!, _mockLogger);
        act.Should().Throw<ArgumentNullException>().WithParameterName("bluetoothService");
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => new BluetoothHidController(_mockBluetoothService, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Fact]
    public async Task SendKeyAsync_WithValidParameters_ShouldReturnTrue()
    {
        // Arrange
        var deviceAddress = "AA:BB:CC:DD:EE:FF";
        var keyCode = HidKeyCode.Home;
        var deviceInfo = new BluetoothDeviceInfo
        {
            Address = deviceAddress,
            Name = "Test HID Device",
            IsConnected = true,
            UuiDs = ["00001124-0000-1000-8000-00805f9b34fb"] // HID Service UUID
        };

        _mockBluetoothService.GetDeviceAsync(deviceAddress, Arg.Any<CancellationToken>())
            .Returns(deviceInfo);

        // Act
        var result = await _hidController.SendKeyAsync(deviceAddress, keyCode);

        // Assert
        result.Should().BeTrue();
        await _mockBluetoothService.Received(1).GetDeviceAsync(deviceAddress, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendKeyAsync_WithNonHidDevice_ShouldReturnFalse()
    {
        // Arrange
        var deviceAddress = "AA:BB:CC:DD:EE:FF";
        var keyCode = HidKeyCode.Home;
        var deviceInfo = new BluetoothDeviceInfo
        {
            Address = deviceAddress,
            Name = "Test Non-HID Device",
            IsConnected = true,
            UuiDs = ["0000180f-0000-1000-8000-00805f9b34fb"] // Battery Service UUID
        };

        _mockBluetoothService.GetDeviceAsync(deviceAddress, Arg.Any<CancellationToken>())
            .Returns(deviceInfo);

        // Act
        var result = await _hidController.SendKeyAsync(deviceAddress, keyCode);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SendKeyAsync_WithDisconnectedDevice_ShouldReturnFalse()
    {
        // Arrange
        var deviceAddress = "AA:BB:CC:DD:EE:FF";
        var keyCode = HidKeyCode.Home;

        _mockBluetoothService.GetDeviceAsync(deviceAddress, Arg.Any<CancellationToken>())
            .Returns((BluetoothDeviceInfo?)null);

        // Act
        var result = await _hidController.SendKeyAsync(deviceAddress, keyCode);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SendKeySequenceAsync_WithValidSequence_ShouldReturnTrue()
    {
        // Arrange
        var deviceAddress = "AA:BB:CC:DD:EE:FF";
        var keyCodes = new[] { HidKeyCode.Home, HidKeyCode.DPadDown, HidKeyCode.DPadCenter };
        var deviceInfo = new BluetoothDeviceInfo
        {
            Address = deviceAddress,
            Name = "Test HID Device",
            IsConnected = true,
            UuiDs = ["00001124-0000-1000-8000-00805f9b34fb"]
        };

        _mockBluetoothService.GetDeviceAsync(deviceAddress, Arg.Any<CancellationToken>())
            .Returns(deviceInfo);

        // Act
        var result = await _hidController.SendKeySequenceAsync(deviceAddress, keyCodes, 10);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SendTextAsync_WithValidText_ShouldReturnTrue()
    {
        // Arrange
        var deviceAddress = "AA:BB:CC:DD:EE:FF";
        var text = "hello";
        var deviceInfo = new BluetoothDeviceInfo
        {
            Address = deviceAddress,
            Name = "Test HID Device",
            IsConnected = true,
            UuiDs = ["00001124-0000-1000-8000-00805f9b34fb"]
        };

        _mockBluetoothService.GetDeviceAsync(deviceAddress, Arg.Any<CancellationToken>())
            .Returns(deviceInfo);

        // Act
        var result = await _hidController.SendTextAsync(deviceAddress, text);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SendTextAsync_WithEmptyText_ShouldReturnTrue()
    {
        // Arrange
        var deviceAddress = "AA:BB:CC:DD:EE:FF";
        var text = "";

        // Act
        var result = await _hidController.SendTextAsync(deviceAddress, text);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ConnectAsync_WithValidAddress_ShouldCallBluetoothService()
    {
        // Arrange
        var deviceAddress = "AA:BB:CC:DD:EE:FF";
        _mockBluetoothService.ConnectDeviceAsync(deviceAddress, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _hidController.ConnectAsync(deviceAddress);

        // Assert
        result.Should().BeTrue();
        await _mockBluetoothService.Received(1).ConnectDeviceAsync(deviceAddress, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DisconnectAsync_WithValidAddress_ShouldCallBluetoothService()
    {
        // Arrange
        var deviceAddress = "AA:BB:CC:DD:EE:FF";
        _mockBluetoothService.DisconnectDeviceAsync(deviceAddress, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _hidController.DisconnectAsync(deviceAddress);

        // Assert
        result.Should().BeTrue();
        await _mockBluetoothService.Received(1).DisconnectDeviceAsync(deviceAddress, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task IsConnectedAsync_WithConnectedDevice_ShouldReturnTrue()
    {
        // Arrange
        var deviceAddress = "AA:BB:CC:DD:EE:FF";
        var deviceInfo = new BluetoothDeviceInfo
        {
            Address = deviceAddress,
            Name = "Test Device",
            IsConnected = true
        };

        _mockBluetoothService.GetDeviceAsync(deviceAddress, Arg.Any<CancellationToken>())
            .Returns(deviceInfo);

        // Act
        var result = await _hidController.IsConnectedAsync(deviceAddress);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsConnectedAsync_WithDisconnectedDevice_ShouldReturnFalse()
    {
        // Arrange
        var deviceAddress = "AA:BB:CC:DD:EE:FF";
        var deviceInfo = new BluetoothDeviceInfo
        {
            Address = deviceAddress,
            Name = "Test Device",
            IsConnected = false
        };

        _mockBluetoothService.GetDeviceAsync(deviceAddress, Arg.Any<CancellationToken>())
            .Returns(deviceInfo);

        // Act
        var result = await _hidController.IsConnectedAsync(deviceAddress);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetConnectedDevicesAsync_WithMultipleDevices_ShouldReturnOnlyHidDevices()
    {
        // Arrange
        var devices = new[]
        {
            new BluetoothDeviceInfo
            {
                Address = "AA:BB:CC:DD:EE:FF",
                Name = "HID Device",
                IsConnected = true,
                UuiDs = ["00001124-0000-1000-8000-00805f9b34fb"]
            },
            new BluetoothDeviceInfo
            {
                Address = "11:22:33:44:55:66",
                Name = "Audio Device",
                IsConnected = true,
                UuiDs = ["0000110b-0000-1000-8000-00805f9b34fb"]
            },
            new BluetoothDeviceInfo
            {
                Address = "99:88:77:66:55:44",
                Name = "Another HID Device",
                IsConnected = false,
                UuiDs = ["00001124-0000-1000-8000-00805f9b34fb"]
            }
        };

        _mockBluetoothService.GetDevicesAsync(Arg.Any<CancellationToken>())
            .Returns(devices);

        // Act
        var result = await _hidController.GetConnectedDevicesAsync();

        // Assert
        result.Should().ContainSingle().Which.Should().Be("AA:BB:CC:DD:EE:FF");
    }
}