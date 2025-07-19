using Moq.Protected;

namespace Zapper.Device.Denon.Tests.Unit;

public class DenonDiscoveryTests
{
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly Mock<HttpMessageHandler> _mockHttpHandler;
    private readonly Mock<ILogger<DenonDiscovery>> _mockLogger;
    private readonly DenonDiscovery _discovery;

    public DenonDiscoveryTests()
    {
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockHttpHandler = new Mock<HttpMessageHandler>();
        _mockLogger = new Mock<ILogger<DenonDiscovery>>();

        var httpClient = new HttpClient(_mockHttpHandler.Object);
        _mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

        _discovery = new DenonDiscovery(_mockHttpClientFactory.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task DiscoverDevicesAsync_ValidDenonDevice_ReturnsDevice()
    {
        var deviceXml = @"
            <Device>
                <ModelName>AVR-X3700H</ModelName>
                <FriendlyName>Living Room AVR</FriendlyName>
                <SerialNumber>ABC123456</SerialNumber>
            </Device>";

        SetupHttpResponse(HttpStatusCode.OK, deviceXml);

        var devices = await _discovery.DiscoverDevicesAsync();

        devices.Should().NotBeEmpty();
        var device = devices.First();
        device.Model.Should().Be("AVR-X3700H");
        device.Name.Should().Be("Living Room AVR");
        device.SerialNumber.Should().Be("ABC123456");
    }

    [Fact]
    public async Task DiscoverDevicesAsync_MarantzDevice_ReturnsDevice()
    {
        var deviceXml = @"
            <Device>
                <ModelName>SR7015</ModelName>
                <FriendlyName>Marantz SR7015</FriendlyName>
                <SerialNumber>XYZ789012</SerialNumber>
            </Device>";

        SetupHttpResponse(HttpStatusCode.OK, deviceXml);

        var devices = await _discovery.DiscoverDevicesAsync();

        devices.Should().NotBeEmpty();
        var device = devices.First();
        device.Model.Should().Be("SR7015");
        device.Name.Should().Be("Marantz SR7015");
    }

    [Fact]
    public async Task DiscoverDevicesAsync_NoDevicesFound_ReturnsEmptyList()
    {
        SetupHttpResponse(HttpStatusCode.NotFound);

        var devices = await _discovery.DiscoverDevicesAsync();

        devices.Should().BeEmpty();
    }

    [Fact]
    public async Task DiscoverDevicesAsync_InvalidXml_ReturnsEmptyList()
    {
        SetupHttpResponse(HttpStatusCode.OK, "invalid xml");

        var devices = await _discovery.DiscoverDevicesAsync();

        devices.Should().BeEmpty();
    }

    [Fact]
    public async Task DiscoverDevicesAsync_NetworkError_ReturnsEmptyList()
    {
        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        var devices = await _discovery.DiscoverDevicesAsync();

        devices.Should().BeEmpty();
    }

    [Fact]
    public async Task DiscoverDevicesAsync_NonDenonDevice_FiltersOut()
    {
        var deviceXml = @"
            <Device>
                <ModelName>Some Other Device</ModelName>
                <FriendlyName>Not a Denon</FriendlyName>
                <SerialNumber>123456</SerialNumber>
            </Device>";

        SetupHttpResponse(HttpStatusCode.OK, deviceXml);

        var devices = await _discovery.DiscoverDevicesAsync();

        devices.Should().BeEmpty();
    }

    [Fact]
    public async Task DiscoverDevicesAsync_MissingModelName_UsesDefault()
    {
        var deviceXml = @"
            <Device>
                <FriendlyName>Denon AVR</FriendlyName>
                <SerialNumber>ABC123</SerialNumber>
            </Device>";

        SetupHttpResponse(HttpStatusCode.OK, deviceXml);

        var devices = await _discovery.DiscoverDevicesAsync();

        devices.Should().NotBeEmpty();
        var device = devices.First();
        device.Model.Should().Be("Unknown Denon Model");
        device.Name.Should().Be("Denon AVR");
    }

    [Fact]
    public async Task DiscoverDevicesAsync_MissingFriendlyName_UsesModelAsName()
    {
        var deviceXml = @"
            <Device>
                <ModelName>Denon AVR-X3700H</ModelName>
                <SerialNumber>ABC123</SerialNumber>
            </Device>";

        SetupHttpResponse(HttpStatusCode.OK, deviceXml);

        var devices = await _discovery.DiscoverDevicesAsync();

        devices.Should().NotBeEmpty();
        var device = devices.First();
        device.Name.Should().Be("Denon AVR-X3700H");
    }

    [Fact]
    public async Task DiscoverDevicesAsync_Timeout_ReturnsEmptyList()
    {
        var tcs = new TaskCompletionSource<HttpResponseMessage>();

        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Returns(async (HttpRequestMessage request, CancellationToken ct) =>
            {
                await Task.Delay(5000, ct);
                return new HttpResponseMessage(HttpStatusCode.OK);
            });

        using var cts = new CancellationTokenSource(100);
        var devices = await _discovery.DiscoverDevicesAsync(cts.Token);

        devices.Should().BeEmpty();
    }

    [Fact]
    public async Task DiscoverDevicesAsync_DuplicateDevices_ReturnsDistinct()
    {
        var deviceXml = @"
            <Device>
                <ModelName>AVR-X3700H</ModelName>
                <FriendlyName>Living Room AVR</FriendlyName>
                <SerialNumber>ABC123456</SerialNumber>
            </Device>";

        var callCount = 0;
        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                callCount++;
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(deviceXml)
                };
            });

        var devices = await _discovery.DiscoverDevicesAsync();

        devices.Should().HaveCount(1);
    }

    private void SetupHttpResponse(HttpStatusCode statusCode, string content = "")
    {
        var response = new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(content)
        };

        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
    }
}