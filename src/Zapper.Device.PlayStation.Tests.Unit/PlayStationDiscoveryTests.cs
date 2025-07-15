using Microsoft.Extensions.Logging;
using Moq;

namespace Zapper.Device.PlayStation.Tests.Unit;

public class PlayStationDiscoveryTests
{
    private readonly Mock<ILogger<PlayStationDiscovery>> _loggerMock;
    private readonly PlayStationDiscovery _discovery;

    public PlayStationDiscoveryTests()
    {
        _loggerMock = new Mock<ILogger<PlayStationDiscovery>>();
        _discovery = new PlayStationDiscovery(_loggerMock.Object);
    }

    [Fact(Timeout = 10000)]
    public async Task DiscoverDevicesAsync_WithTimeout_ReturnsEmptyList()
    {
        var timeout = TimeSpan.FromSeconds(1);

        var devices = await _discovery.DiscoverDevicesAsync(timeout);

        Assert.NotNull(devices);
        Assert.Empty(devices); // Will be empty without actual PlayStation devices
    }

    [Fact(Timeout = 5000)]
    public void DeviceDiscovered_Event_CanBeSubscribed()
    {
        var eventRaised = false;
        _discovery.DeviceDiscovered += (sender, device) => eventRaised = true;

        // Event should be subscribable
        Assert.False(eventRaised); // No events should be raised during construction
    }

    [Fact]
    public void Dispose_DoesNotThrow()
    {
        var exception = Record.Exception(() => _discovery.Dispose());
        Assert.Null(exception);
    }
}