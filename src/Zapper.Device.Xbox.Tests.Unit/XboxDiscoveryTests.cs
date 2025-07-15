using Microsoft.Extensions.Logging;
using Moq;
using Zapper.Device.Xbox;
using Zapper.Device.Xbox.Models;

namespace Zapper.Device.Xbox.Tests.Unit;

public class XboxDiscoveryTests
{
    private readonly Mock<ILogger<XboxDiscovery>> _loggerMock;
    private readonly XboxDiscovery _discovery;

    public XboxDiscoveryTests()
    {
        _loggerMock = new Mock<ILogger<XboxDiscovery>>();
        _discovery = new XboxDiscovery(_loggerMock.Object);
    }

    [Fact]
    public async Task DiscoverDevicesAsync_WhenCancelled_ReturnsEmptyList()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await _discovery.DiscoverDevicesAsync(TimeSpan.FromSeconds(5), cts.Token);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task DiscoverDevicesAsync_WithShortTimeout_CompletesWithoutException()
    {
        var result = await _discovery.DiscoverDevicesAsync(TimeSpan.FromMilliseconds(100), CancellationToken.None);

        Assert.NotNull(result);
        _loggerMock.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Starting Xbox console discovery")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void DeviceFound_Event_CanBeSubscribed()
    {
        var eventRaised = false;
        _discovery.DeviceFound += (sender, device) => eventRaised = true;

        Assert.False(eventRaised);
    }
}