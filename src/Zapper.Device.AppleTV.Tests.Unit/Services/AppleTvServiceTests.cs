using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Zapper.Core.Models;
using Zapper.Data;
using Zapper.Device.AppleTV.Interfaces;
using Zapper.Device.AppleTV.Models;
using Zapper.Device.AppleTV.Services;

namespace Zapper.Device.AppleTV.Tests.Unit.Services;

public class AppleTvServiceTests : IDisposable
{
    private readonly ILogger<AppleTvService> _mockLogger;
    private readonly AppleTvDiscoveryService _mockDiscoveryService;
    private readonly AppleTvControllerFactory _mockControllerFactory;
    private readonly ZapperContext _context;
    private readonly AppleTvService _service;
    private readonly IAppleTvController _mockController;

    public AppleTvServiceTests()
    {
        _mockLogger = Substitute.For<ILogger<AppleTvService>>();
        _mockDiscoveryService = Substitute.For<AppleTvDiscoveryService>(Substitute.For<ILogger<AppleTvDiscoveryService>>());
        _mockControllerFactory = Substitute.For<AppleTvControllerFactory>(
            Substitute.For<IServiceProvider>(),
            Substitute.For<ILogger<AppleTvControllerFactory>>());

        // Setup in-memory database
        var options = new DbContextOptionsBuilder<ZapperContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ZapperContext(options);

        _mockController = Substitute.For<IAppleTvController>();

        _service = new AppleTvService(_mockLogger, _mockDiscoveryService, _mockControllerFactory, _context);
    }

    [Fact]
    public async Task DiscoverAppleTvsAsync_CallsDiscoveryService()
    {
        // Arrange
        var expectedDevices = new List<AppleTvDevice>
        {
            new AppleTvDevice
            {
                Name = "Living Room Apple TV",
                IpAddress = "192.168.1.100",
                Port = 7000,
                Identifier = "device1"
            },
            new AppleTvDevice
            {
                Name = "Bedroom Apple TV",
                IpAddress = "192.168.1.101",
                Port = 7000,
                Identifier = "device2"
            }
        };

        _mockDiscoveryService.DiscoverDevicesAsync(5).Returns(expectedDevices);

        // Act
        var result = await _service.DiscoverAppleTvsAsync(5);

        // Assert
        result.Should().Equal(expectedDevices);
        await _mockDiscoveryService.Received(1).DiscoverDevicesAsync(5);
    }

    [Fact]
    public async Task CreateDeviceFromDiscoveredAsync_ValidDevice_CreatesAndSavesDevice()
    {
        // Arrange
        var discoveredDevice = new AppleTvDevice
        {
            Name = "Test Apple TV",
            IpAddress = "192.168.1.100",
            Port = 7000,
            Model = "Apple TV 4K",
            Identifier = "ABC123",
            Version = "16.0",
            RequiresPairing = true,
            IsPaired = false,
            PreferredProtocol = ConnectionType.CompanionProtocol,
            Properties = new Dictionary<string, string> { { "key", "value" } }
        };

        // Act
        var result = await _service.CreateDeviceFromDiscoveredAsync(discoveredDevice);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Apple TV");
        result.Brand.Should().Be("Apple");
        result.Model.Should().Be("Apple TV 4K");
        result.Type.Should().Be(DeviceType.StreamingDevice);
        result.ConnectionType.Should().Be(ConnectionType.CompanionProtocol);
        result.IpAddress.Should().Be("192.168.1.100");
        result.Port.Should().Be(7000);
        result.DeviceIdentifier.Should().Be("ABC123");
        result.ProtocolVersion.Should().Be("16.0");
        result.RequiresPairing.Should().BeTrue();
        result.IsPaired.Should().BeFalse();
        result.IsOnline.Should().BeTrue();
        result.SupportsKeyboardInput.Should().BeTrue();
        result.SupportsMouseInput.Should().BeFalse();

        _context.Devices.Count().Should().Be(1);
    }

    [Fact]
    public async Task CreateDeviceFromDiscoveredAsync_ExceptionThrown_ReturnsNull()
    {
        // Arrange
        var discoveredDevice = new AppleTvDevice
        {
            Name = null!, // This will cause an exception
            IpAddress = "192.168.1.100"
        };

        // Act
        var result = await _service.CreateDeviceFromDiscoveredAsync(discoveredDevice);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ConnectToDeviceAsync_NewDevice_CreatesControllerAndConnects()
    {
        // Arrange
        var device = new Zapper.Core.Models.Device
        {
            Id = 1,
            Name = "Test Apple TV",
            IpAddress = "192.168.1.100",
            ConnectionType = ConnectionType.MediaRemoteProtocol
        };

        _mockControllerFactory.CreateControllerForDevice(device).Returns(_mockController);
        _mockController.ConnectAsync(device).Returns(true);

        // Act
        var result = await _service.ConnectToDeviceAsync(device);

        // Assert
        result.Should().BeTrue();
        _mockControllerFactory.Received(1).CreateControllerForDevice(device);
        await _mockController.Received(1).ConnectAsync(device);
    }

    [Fact]
    public async Task ConnectToDeviceAsync_AlreadyConnected_ReturnsTrue()
    {
        // Arrange
        var device = new Zapper.Core.Models.Device
        {
            Id = 1,
            Name = "Test Apple TV",
            IpAddress = "192.168.1.100"
        };

        // First connection
        _mockControllerFactory.CreateControllerForDevice(device).Returns(_mockController);
        _mockController.ConnectAsync(device).Returns(true);
        await _service.ConnectToDeviceAsync(device);

        // Act - Try to connect again
        var result = await _service.ConnectToDeviceAsync(device);

        // Assert
        result.Should().BeTrue();
        _mockControllerFactory.Received(1).CreateControllerForDevice(device); // Only called once
    }

    [Fact]
    public async Task ConnectToDeviceAsync_ConnectionFails_ReturnsFalse()
    {
        // Arrange
        var device = new Zapper.Core.Models.Device
        {
            Id = 1,
            Name = "Test Apple TV",
            IpAddress = "192.168.1.100"
        };

        _mockControllerFactory.CreateControllerForDevice(device).Returns(_mockController);
        _mockController.ConnectAsync(device).Returns(false);

        // Act
        var result = await _service.ConnectToDeviceAsync(device);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DisconnectFromDeviceAsync_ConnectedDevice_DisconnectsSuccessfully()
    {
        // Arrange
        var device = new Zapper.Core.Models.Device
        {
            Id = 1,
            Name = "Test Apple TV",
            IpAddress = "192.168.1.100"
        };

        // First connect
        _mockControllerFactory.CreateControllerForDevice(device).Returns(_mockController);
        _mockController.ConnectAsync(device).Returns(true);
        await _service.ConnectToDeviceAsync(device);

        _mockController.DisconnectAsync().Returns(true);

        // Act
        var result = await _service.DisconnectFromDeviceAsync(device.Id);

        // Assert
        result.Should().BeTrue();
        await _mockController.Received(1).DisconnectAsync();
    }

    [Fact]
    public async Task DisconnectFromDeviceAsync_NotConnected_ReturnsTrue()
    {
        // Act
        var result = await _service.DisconnectFromDeviceAsync(999);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SendCommandAsync_ConnectedDevice_SendsCommand()
    {
        // Arrange
        var device = new Zapper.Core.Models.Device
        {
            Id = 1,
            Name = "Test Apple TV",
            IpAddress = "192.168.1.100"
        };

        var command = new DeviceCommand
        {
            Name = "volumeup",
            Type = CommandType.VolumeUp
        };

        // Connect first
        _mockControllerFactory.CreateControllerForDevice(device).Returns(_mockController);
        _mockController.ConnectAsync(device).Returns(true);
        await _service.ConnectToDeviceAsync(device);

        _mockController.SendCommandAsync(command).Returns(true);

        // Act
        var result = await _service.SendCommandAsync(device.Id, command);

        // Assert
        result.Should().BeTrue();
        await _mockController.Received(1).SendCommandAsync(command);
    }

    [Fact]
    public async Task SendCommandAsync_NotConnected_ReturnsFalse()
    {
        // Arrange
        var command = new DeviceCommand { Name = "volumeup" };

        // Act
        var result = await _service.SendCommandAsync(999, command);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetStatusAsync_ConnectedDevice_ReturnsStatus()
    {
        // Arrange
        var device = new Zapper.Core.Models.Device
        {
            Id = 1,
            Name = "Test Apple TV",
            IpAddress = "192.168.1.100"
        };

        var expectedStatus = new AppleTvStatus
        {
            IsPlaying = true,
            CurrentApp = "Netflix",
            PlaybackState = PlaybackState.Playing
        };

        // Connect first
        _mockControllerFactory.CreateControllerForDevice(device).Returns(_mockController);
        _mockController.ConnectAsync(device).Returns(true);
        await _service.ConnectToDeviceAsync(device);

        _mockController.GetStatusAsync().Returns(expectedStatus);

        // Act
        var result = await _service.GetStatusAsync(device.Id);

        // Assert
        result.Should().Be(expectedStatus);
    }

    [Fact]
    public async Task PairDeviceAsync_ValidPin_CallsPairMethod()
    {
        // Arrange
        var device = new Zapper.Core.Models.Device
        {
            Id = 1,
            Name = "Test Apple TV",
            IpAddress = "192.168.1.100",
            Brand = "Apple",
            Type = DeviceType.StreamingDevice
        };

        // Add device to context first
        _context.Devices.Add(device);
        await _context.SaveChangesAsync();

        var pin = "1234";

        // Connect first
        _mockControllerFactory.CreateControllerForDevice(device).Returns(_mockController);
        _mockController.ConnectAsync(device).Returns(true);
        var connected = await _service.ConnectToDeviceAsync(device);
        connected.Should().BeTrue(); // Verify connection succeeded

        _mockController.PairAsync(pin).Returns(true);

        // Act
        var result = await _service.PairDeviceAsync(device.Id, pin);

        // Assert
        result.Should().BeTrue();
        await _mockController.Received(1).PairAsync(pin);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}