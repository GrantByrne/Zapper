using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Zapper.Device.Sonos.Tests.Unit;

public class SonosDiscoveryTests
{
    private readonly ILogger<SonosDiscovery> _loggerMock;
    private readonly HttpClient _httpClient;
    private readonly SonosDiscovery _discovery;

    public SonosDiscoveryTests()
    {
        _loggerMock = Substitute.For<ILogger<SonosDiscovery>>();
        _httpClient = new HttpClient();
        _discovery = new SonosDiscovery(_loggerMock, _httpClient);
    }

    [Fact(Timeout = 10000)]
    public async Task DiscoverDevices_WithTimeout_ReturnsEmptyList()
    {
        var timeout = TimeSpan.FromSeconds(1);

        var devices = await _discovery.DiscoverDevices(timeout);

        Assert.NotNull(devices);
        Assert.Empty(devices); // Will be empty without actual Sonos devices
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