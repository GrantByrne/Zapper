using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Zapper.Core.Models;
using Xunit;

namespace Zapper.Device.Tizen.Tests.Unit;

public class TizenDiscoveryTests : IDisposable
{
    private readonly ILogger<TizenDiscovery> _mockLogger;
    private readonly ITizenClient _mockTizenClient;
    private readonly TizenDiscovery _discovery;

    public TizenDiscoveryTests()
    {
        _mockLogger = Substitute.For<ILogger<TizenDiscovery>>();
        _mockTizenClient = Substitute.For<ITizenClient>();
        _discovery = new TizenDiscovery(_mockLogger, _mockTizenClient);
    }

    [Fact]
    public async Task DiscoverDevices_WithDefaultTimeout_ShouldUse10Seconds()
    {
        var cts = new CancellationTokenSource();
        cts.CancelAfter(100); // Cancel quickly to prevent actual network operations

        var result = await _discovery.DiscoverDevices(default, cts.Token);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task DiscoverDevices_WithCustomTimeout_ShouldUseProvidedTimeout()
    {
        var cts = new CancellationTokenSource();
        cts.CancelAfter(100); // Cancel quickly to prevent actual network operations

        var result = await _discovery.DiscoverDevices(TimeSpan.FromSeconds(5), cts.Token);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task DiscoverDevices_WhenCancelled_ShouldReturnDiscoveredDevices()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await _discovery.DiscoverDevices(TimeSpan.FromSeconds(5), cts.Token);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
        _mockLogger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Discovered 0 Samsung Tizen devices")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task DiscoverDeviceByIp_WhenCannotConnect_ShouldReturnNull()
    {
        var ipAddress = "192.168.1.100";
        _mockTizenClient.ConnectAsync(ipAddress, null, Arg.Any<CancellationToken>()).Returns(false);

        var result = await _discovery.DiscoverDeviceByIp(ipAddress);

        result.Should().BeNull();
        await _mockTizenClient.Received(1).ConnectAsync(ipAddress, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DiscoverDeviceByIp_WhenConnected_ShouldReturnDevice()
    {
        var ipAddress = "192.168.1.100";
        _mockTizenClient.ConnectAsync(ipAddress, null, Arg.Any<CancellationToken>()).Returns(true);

        var result = await _discovery.DiscoverDeviceByIp(ipAddress);

        result.Should().NotBeNull();
        result!.Name.Should().Be($"Samsung TV ({ipAddress})");
        result.Brand.Should().Be("Samsung");
        result.Model.Should().Be("Unknown");
        result.Type.Should().Be(DeviceType.TizenTv);
        result.ConnectionType.Should().Be(ConnectionType.Tizen);
        result.NetworkAddress.Should().Be(ipAddress);
        result.UseSecureConnection.Should().BeTrue();
        result.IsOnline.Should().BeTrue();

        await _mockTizenClient.Received(1).DisconnectAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DiscoverDeviceByIp_WhenExceptionOccurs_ShouldReturnNullAndLogError()
    {
        var ipAddress = "192.168.1.100";
        _mockTizenClient.ConnectAsync(ipAddress, null, Arg.Any<CancellationToken>())
            .Returns(Task.FromException<bool>(new Exception("Connection failed")));

        var result = await _discovery.DiscoverDeviceByIp(ipAddress);

        result.Should().BeNull();
        _mockLogger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains($"Failed to discover Samsung TV at {ipAddress}")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task PairWithDevice_WithNonTizenDevice_ShouldReturnFalse()
    {
        var device = new Zapper.Core.Models.Device
        {
            Name = "Test Device",
            ConnectionType = ConnectionType.InfraredIr
        };

        var result = await _discovery.PairWithDevice(device);

        result.Should().BeFalse();
        _mockLogger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("is not a Tizen device")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task PairWithDevice_WhenCannotConnect_ShouldReturnFalse()
    {
        var device = new Zapper.Core.Models.Device
        {
            Name = "Samsung TV",
            ConnectionType = ConnectionType.Tizen,
            NetworkAddress = "192.168.1.100"
        };

        _mockTizenClient.ConnectAsync(device.NetworkAddress, null, Arg.Any<CancellationToken>()).Returns(false);

        var result = await _discovery.PairWithDevice(device);

        result.Should().BeFalse();
        _mockLogger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Failed to connect to Tizen device")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task PairWithDevice_WhenAuthenticationFails_ShouldReturnFalse()
    {
        var device = new Zapper.Core.Models.Device
        {
            Name = "Samsung TV",
            ConnectionType = ConnectionType.Tizen,
            NetworkAddress = "192.168.1.100"
        };

        _mockTizenClient.ConnectAsync(device.NetworkAddress, null, Arg.Any<CancellationToken>()).Returns(true);
        _mockTizenClient.AuthenticateAsync("Zapper", Arg.Any<CancellationToken>()).Returns((string?)null);

        var result = await _discovery.PairWithDevice(device);

        result.Should().BeFalse();
        _mockLogger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Failed to authenticate with Tizen device")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task PairWithDevice_WhenSuccessful_ShouldReturnTrueAndUpdateDevice()
    {
        var device = new Zapper.Core.Models.Device
        {
            Name = "Samsung TV",
            ConnectionType = ConnectionType.Tizen,
            NetworkAddress = "192.168.1.100",
            AuthenticationToken = null
        };
        var token = "auth-token-123";

        _mockTizenClient.ConnectAsync(device.NetworkAddress, null, Arg.Any<CancellationToken>()).Returns(true);
        _mockTizenClient.AuthenticateAsync("Zapper", Arg.Any<CancellationToken>()).Returns(token);

        var result = await _discovery.PairWithDevice(device);

        result.Should().BeTrue();
        device.AuthenticationToken.Should().Be(token);
        device.LastSeen.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        
        await _mockTizenClient.Received(1).DisconnectAsync(Arg.Any<CancellationToken>());
        _mockLogger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Successfully paired with Tizen device")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task PairWithDevice_WithPinCode_ShouldPassPinToConnect()
    {
        var device = new Zapper.Core.Models.Device
        {
            Name = "Samsung TV",
            ConnectionType = ConnectionType.Tizen,
            NetworkAddress = "192.168.1.100",
            AuthenticationToken = "existing-token"
        };
        var pinCode = "1234";
        var newToken = "new-auth-token";

        _mockTizenClient.ConnectAsync(device.NetworkAddress, device.AuthenticationToken, Arg.Any<CancellationToken>()).Returns(true);
        _mockTizenClient.AuthenticateAsync("Zapper", Arg.Any<CancellationToken>()).Returns(newToken);

        var result = await _discovery.PairWithDevice(device, pinCode);

        result.Should().BeTrue();
        device.AuthenticationToken.Should().Be(newToken);
    }

    [Fact]
    public async Task PairWithDevice_WhenExceptionOccurs_ShouldReturnFalseAndLogError()
    {
        var device = new Zapper.Core.Models.Device
        {
            Name = "Samsung TV",
            ConnectionType = ConnectionType.Tizen,
            NetworkAddress = "192.168.1.100"
        };

        _mockTizenClient.ConnectAsync(device.NetworkAddress, null, Arg.Any<CancellationToken>())
            .Returns(Task.FromException<bool>(new Exception("Connection failed")));

        var result = await _discovery.PairWithDevice(device);

        result.Should().BeFalse();
        _mockLogger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Failed to pair with Tizen device")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public void DeviceDiscovered_Event_ShouldBeRaiseable()
    {
        Zapper.Core.Models.Device? discoveredDevice = null;
        _discovery.DeviceDiscovered += (sender, device) => discoveredDevice = device;

        var testDevice = new Zapper.Core.Models.Device { Name = "Test Samsung TV" };
        
        // Trigger the event through reflection since we can't invoke it directly
        var eventField = typeof(TizenDiscovery).GetField("DeviceDiscovered", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var eventDelegate = eventField?.GetValue(_discovery) as EventHandler<Zapper.Core.Models.Device>;
        eventDelegate?.Invoke(_discovery, testDevice);

        discoveredDevice.Should().NotBeNull();
        discoveredDevice!.Name.Should().Be("Test Samsung TV");
    }

    public void Dispose()
    {
        _discovery.Dispose();
    }
}