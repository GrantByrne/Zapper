using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Zapper.Device.WebOS.Tests.Unit;

public class WebOsDiscoveryTests
{
    private readonly ILogger<WebOsDiscovery> _mockLogger;
    private readonly IWebOsClient _mockWebOsClient;
    private readonly WebOsDiscovery _discovery;

    public WebOsDiscoveryTests()
    {
        _mockLogger = Substitute.For<ILogger<WebOsDiscovery>>();
        _mockWebOsClient = Substitute.For<IWebOsClient>();
        _discovery = new WebOsDiscovery(_mockLogger, _mockWebOsClient);
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