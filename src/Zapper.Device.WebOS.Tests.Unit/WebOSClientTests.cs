using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Zapper.Device.WebOS.Tests.Unit;

public class WebOSClientTests
{
    private readonly ILogger<WebOSClient> _mockLogger;
    private readonly WebOSClient _client;

    public WebOSClientTests()
    {
        _mockLogger = Substitute.For<ILogger<WebOSClient>>();
        _client = new WebOSClient(_mockLogger);
    }

    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Act & Assert
        _client.IsConnected.Should().BeFalse();
        _client.ClientKey.Should().BeNull();
    }

    [Fact]
    public async Task PowerOffAsync_ShouldReturnFalseWhenNotConnected()
    {
        // Act
        var result = await _client.PowerOffAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task VolumeUpAsync_ShouldReturnFalseWhenNotConnected()
    {
        // Act
        var result = await _client.VolumeUpAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SetVolumeAsync_WithValidVolume_ShouldReturnFalseWhenNotConnected()
    {
        // Act
        var result = await _client.SetVolumeAsync(50);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task LaunchAppAsync_WithValidAppId_ShouldReturnFalseWhenNotConnected()
    {
        // Act
        var result = await _client.LaunchAppAsync("netflix");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Dispose_ShouldNotThrowException()
    {
        // Act & Assert
        var act = () => _client.Dispose();
        act.Should().NotThrow();
    }
}