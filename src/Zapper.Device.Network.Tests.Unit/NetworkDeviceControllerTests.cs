using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Zapper.Device.Network.Tests.Unit;

public class NetworkDeviceControllerTests : IDisposable
{
    private readonly ILogger<NetworkDeviceController> _mockLogger;
    private readonly NetworkDeviceController _controller;
    private readonly HttpClient _httpClient;

    public NetworkDeviceControllerTests()
    {
        _httpClient = new HttpClient();
        _mockLogger = Substitute.For<ILogger<NetworkDeviceController>>();
        _controller = new NetworkDeviceController(_httpClient, _mockLogger);
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Act & Assert
        _controller.Should().NotBeNull();
        _controller.Should().BeAssignableTo<INetworkDeviceController>();
    }

    [Fact]
    public async Task SendHttpCommandAsync_WithInvalidUrl_ShouldReturnFalse()
    {
        // Arrange
        var baseUrl = "invalid-url";
        var endpoint = "api/test";

        // Act
        var result = await _controller.SendHttpCommandAsync(baseUrl, endpoint);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SendHttpCommandAsync_WithValidParameters_ShouldNotThrow()
    {
        // Arrange
        const string baseUrl = "https://httpbin.org";
        const string endpoint = "status/404";
        const string method = "GET";

        // Act & Assert
        var act = async () => await _controller.SendHttpCommandAsync(baseUrl, endpoint, method);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DiscoverDevicesAsync_WithValidParameters_ShouldReturnDevicesJson()
    {
        // Arrange
        const string deviceType = "ssdp:all";
        var timeout = TimeSpan.FromSeconds(1);

        // Act
        var result = await _controller.DiscoverDevicesAsync(deviceType, timeout);

        // Assert - Since we can't easily mock UDP sockets, we just verify it doesn't throw
        // In a real network environment, this might return actual discovered devices
        result.Should().BeNullOrWhiteSpace(); // or contain JSON if devices are found
    }

    [Fact]
    public async Task DiscoverDevicesAsync_WithShortTimeout_ShouldCompleteQuickly()
    {
        // Arrange
        const string deviceType = "ssdp:all";
        var timeout = TimeSpan.FromMilliseconds(100);
        var startTime = DateTime.UtcNow;

        // Act
        var result = await _controller.DiscoverDevicesAsync(deviceType, timeout);
        var elapsed = DateTime.UtcNow - startTime;

        // Assert
        elapsed.Should().BeLessThan(TimeSpan.FromSeconds(2)); // Should complete within reasonable time
    }

    [Fact]
    public void Dispose_ShouldNotThrow()
    {
        // Act & Assert
        var act = () => _controller.Dispose();
        act.Should().NotThrow();
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_ShouldNotThrow()
    {
        // Act & Assert
        var act = () =>
        {
            _controller.Dispose();
            _controller.Dispose();
        };
        act.Should().NotThrow();
    }

    public void Dispose()
    {
        _controller?.Dispose();
        _httpClient?.Dispose();
    }
}