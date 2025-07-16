using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Zapper.Device.PlayStation.Tests.Unit;

public class PlayStationDiscoveryTests
{
    private readonly ILogger<PlayStationDiscovery> _logger;
    private readonly PlayStationDiscovery _discovery;

    public PlayStationDiscoveryTests()
    {
        _logger = Substitute.For<ILogger<PlayStationDiscovery>>();
        _discovery = new PlayStationDiscovery(_logger);
    }

    [Fact(Timeout = 10000)]
    public async Task DiscoverDevices_WithTimeout_ReturnsEmptyList()
    {
        var timeout = TimeSpan.FromSeconds(1);

        var devices = await _discovery.DiscoverDevices(timeout);

        Assert.NotNull(devices);
        Assert.Empty(devices); // Will be empty without actual PlayStation devices
    }

    [Fact]
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