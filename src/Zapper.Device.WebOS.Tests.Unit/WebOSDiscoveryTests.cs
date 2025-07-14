using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Zapper.Device.WebOS.Tests.Unit;

public class WebOSDiscoveryTests
{
    private readonly Mock<ILogger<WebOSDiscovery>> _mockLogger;
    private readonly Mock<IWebOSClient> _mockWebOSClient;
    private readonly WebOSDiscovery _discovery;

    public WebOSDiscoveryTests()
    {
        _mockLogger = new Mock<ILogger<WebOSDiscovery>>();
        _mockWebOSClient = new Mock<IWebOSClient>();
        _discovery = new WebOSDiscovery(_mockLogger.Object, _mockWebOSClient.Object);
    }

    [Fact]
    public void Constructor_ShouldInitializeCorrectly()
    {
        // Act & Assert
        _discovery.Should().NotBeNull();
    }

    [Fact]
    public async Task DiscoverDevicesAsync_WithDefaultTimeout_ShouldComplete()
    {
        // Act
        var result = await _discovery.DiscoverDevicesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty(); // In test environment, no devices found
    }

    [Fact]
    public async Task DiscoverDevicesAsync_WithCancellation_ShouldRespectCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var result = await _discovery.DiscoverDevicesAsync(TimeSpan.FromSeconds(5), cts.Token);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty(); // Should return empty list when cancelled immediately
    }

    [Fact]
    public async Task DiscoverDevicesAsync_WithShortTimeout_ShouldCompleteQuickly()
    {
        // Arrange
        var shortTimeout = TimeSpan.FromMilliseconds(100);
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var result = await _discovery.DiscoverDevicesAsync(shortTimeout);
        stopwatch.Stop();

        // Assert
        result.Should().NotBeNull();
        // Should complete within reasonable time (allowing for test environment overhead)
        stopwatch.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Dispose_ShouldNotThrowException()
    {
        // Act & Assert
        var act = () => _discovery.Dispose();
        act.Should().NotThrow();
    }
}