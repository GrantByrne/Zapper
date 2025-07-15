using Microsoft.Extensions.Logging;
using Moq;
using Zapper.Device.Xbox.Network;

namespace Zapper.Device.Xbox.Tests.Unit;

public class XboxDiscoveryTests
{
    private readonly Mock<INetworkClientFactory> _networkClientFactoryMock;
    private readonly Mock<IUdpClientWrapper> _udpClientMock;
    private readonly Mock<ILogger<XboxDiscovery>> _loggerMock;
    private readonly XboxDiscovery _discovery;

    public XboxDiscoveryTests()
    {
        _networkClientFactoryMock = new Mock<INetworkClientFactory>();
        _udpClientMock = new Mock<IUdpClientWrapper>();
        _loggerMock = new Mock<ILogger<XboxDiscovery>>();

        _networkClientFactoryMock.Setup(x => x.CreateUdpClient()).Returns(_udpClientMock.Object);
        _discovery = new XboxDiscovery(_networkClientFactoryMock.Object, _loggerMock.Object);
    }

    [Fact(Timeout = 5000)]
    public async Task DiscoverDevicesAsync_WhenCancelled_ReturnsEmptyList()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await _discovery.DiscoverDevicesAsync(TimeSpan.FromSeconds(5), cts.Token);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact(Timeout = 5000)]
    public async Task DiscoverDevicesAsync_WithShortTimeout_CompletesWithoutException()
    {
        _udpClientMock.Setup(x => x.ReceiveAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

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