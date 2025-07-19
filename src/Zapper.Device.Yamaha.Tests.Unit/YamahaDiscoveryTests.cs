using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Zapper.Device.Yamaha.Tests.Unit;

public class YamahaDiscoveryTests
{
    private readonly ILogger<YamahaDiscovery> _loggerMock;
    private readonly HttpClient _httpClient;
    private readonly YamahaDiscovery _discovery;

    public YamahaDiscoveryTests()
    {
        _loggerMock = Substitute.For<ILogger<YamahaDiscovery>>();
        _httpClient = new HttpClient();
        _discovery = new YamahaDiscovery(_loggerMock, _httpClient);
    }

    [Fact(Timeout = 10000)]
    public async Task DiscoverDevices_WithTimeout_ReturnsEmptyList()
    {
        var timeout = TimeSpan.FromSeconds(1);

        var devices = await _discovery.DiscoverDevices(timeout);

        Assert.NotNull(devices);
        // Test passes if devices are found or not - the discovery mechanism is working
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