using Microsoft.Extensions.Logging;
using NSubstitute;
using Zapper.Device.Xbox.Network;

namespace Zapper.Device.Xbox.Tests.Unit;

public class XboxDiscoveryTests
{
    private readonly INetworkClientFactory _networkClientFactoryMock;
    private readonly IUdpClientWrapper _udpClientMock;
    private readonly ILogger<XboxDiscovery> _loggerMock;
    private readonly XboxDiscovery _discovery;

    public XboxDiscoveryTests()
    {
        _networkClientFactoryMock = Substitute.For<INetworkClientFactory>();
        _udpClientMock = Substitute.For<IUdpClientWrapper>();
        _loggerMock = Substitute.For<ILogger<XboxDiscovery>>();

        _networkClientFactoryMock.CreateUdpClient().Returns(_udpClientMock);
        _discovery = new XboxDiscovery(_networkClientFactoryMock, _loggerMock);
    }

    [Fact(Timeout = 5000)]
    public async Task DiscoverDevices_WhenCancelled_ReturnsEmptyList()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await _discovery.DiscoverDevices(TimeSpan.FromSeconds(5), cts.Token);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact(Timeout = 5000)]
    public async Task DiscoverDevices_WithShortTimeout_CompletesWithoutException()
    {
        _udpClientMock.ReceiveAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException<System.Net.Sockets.UdpReceiveResult>(new OperationCanceledException()));

        var result = await _discovery.DiscoverDevices(TimeSpan.FromMilliseconds(100), CancellationToken.None);

        Assert.NotNull(result);
        _loggerMock.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(v => v.ToString()!.Contains("Starting Xbox console discovery")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public void DeviceFound_Event_CanBeSubscribed()
    {
        var eventRaised = false;
        _discovery.DeviceFound += (sender, device) => eventRaised = true;

        Assert.False(eventRaised);
    }
}