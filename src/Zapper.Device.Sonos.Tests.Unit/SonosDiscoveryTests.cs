using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;

namespace Zapper.Device.Sonos.Tests.Unit;

public class SonosDiscoveryTests
{
    private readonly Mock<ILogger<SonosDiscovery>> _loggerMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly SonosDiscovery _discovery;

    public SonosDiscoveryTests()
    {
        _loggerMock = new Mock<ILogger<SonosDiscovery>>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        _discovery = new SonosDiscovery(_loggerMock.Object, _httpClient);
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

    private void SetupHttpResponse(HttpStatusCode statusCode, string content)
    {
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content)
            });
    }
}