using Moq.Protected;

namespace Zapper.Device.Denon.Tests.Unit;

public class DenonDeviceControllerTests
{
    private readonly Mock<HttpMessageHandler> _mockHttpHandler;
    private readonly Mock<ILogger<DenonDeviceController>> _mockLogger;
    private readonly HttpClient _httpClient;
    private readonly DenonDeviceController _controller;

    public DenonDeviceControllerTests()
    {
        _mockHttpHandler = new Mock<HttpMessageHandler>();
        _mockLogger = new Mock<ILogger<DenonDeviceController>>();
        _httpClient = new HttpClient(_mockHttpHandler.Object)
        {
            BaseAddress = new Uri("http://192.168.1.100/")
        };
        _controller = new DenonDeviceController(_httpClient, _mockLogger.Object);
    }

    [Fact]
    public async Task SetPowerAsync_PowerOn_SendsCorrectCommand()
    {
        SetupHttpResponse(HttpStatusCode.OK);

        var result = await _controller.SetPowerAsync(true);

        result.Should().BeTrue();
        VerifyHttpRequest("/goform/formiPhoneAppDirect.xml?PWON");
    }

    [Fact]
    public async Task SetPowerAsync_PowerOff_SendsCorrectCommand()
    {
        SetupHttpResponse(HttpStatusCode.OK);

        var result = await _controller.SetPowerAsync(false);

        result.Should().BeTrue();
        VerifyHttpRequest("/goform/formiPhoneAppDirect.xml?PWSTANDBY");
    }

    [Fact]
    public async Task GetPowerStatusAsync_PowerOn_ReturnsTrue()
    {
        SetupHttpPostResponse("<cmd>PWON</cmd>");

        var result = await _controller.GetPowerStatusAsync();

        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetPowerStatusAsync_PowerOff_ReturnsFalse()
    {
        SetupHttpPostResponse("<cmd>PWSTANDBY</cmd>");

        var result = await _controller.GetPowerStatusAsync();

        result.Should().BeFalse();
    }

    [Fact]
    public async Task SetVolumeAsync_ValidVolume_SendsCorrectCommand()
    {
        SetupHttpResponse(HttpStatusCode.OK);

        var result = await _controller.SetVolumeAsync(50);

        result.Should().BeTrue();
        VerifyHttpRequest("/goform/formiPhoneAppDirect.xml?MV40");
    }

    [Fact]
    public async Task SetVolumeAsync_VolumeAboveMax_ClampsToMax()
    {
        SetupHttpResponse(HttpStatusCode.OK);

        var result = await _controller.SetVolumeAsync(120);

        result.Should().BeTrue();
        VerifyHttpRequest("/goform/formiPhoneAppDirect.xml?MV78");
    }

    [Fact]
    public async Task GetVolumeAsync_ReturnsCorrectVolume()
    {
        SetupHttpPostResponse("<cmd>MV40</cmd>");

        var result = await _controller.GetVolumeAsync();

        result.Should().Be(50);
    }

    [Fact]
    public async Task SetMuteAsync_MuteOn_SendsCorrectCommand()
    {
        SetupHttpResponse(HttpStatusCode.OK);

        var result = await _controller.SetMuteAsync(true);

        result.Should().BeTrue();
        VerifyHttpRequest("/goform/formiPhoneAppDirect.xml?MUON");
    }

    [Fact]
    public async Task SetMuteAsync_MuteOff_SendsCorrectCommand()
    {
        SetupHttpResponse(HttpStatusCode.OK);

        var result = await _controller.SetMuteAsync(false);

        result.Should().BeTrue();
        VerifyHttpRequest("/goform/formiPhoneAppDirect.xml?MUOFF");
    }

    [Fact]
    public async Task GetMuteStatusAsync_MuteOn_ReturnsTrue()
    {
        SetupHttpPostResponse("<cmd>MUON</cmd>");

        var result = await _controller.GetMuteStatusAsync();

        result.Should().BeTrue();
    }

    [Fact]
    public async Task SetInputAsync_ValidInput_SendsCorrectCommand()
    {
        SetupHttpResponse(HttpStatusCode.OK);

        var result = await _controller.SetInputAsync("DVD");

        result.Should().BeTrue();
        VerifyHttpRequest("/goform/formiPhoneAppDirect.xml?SIDVD");
    }

    [Fact]
    public async Task SetInputAsync_FriendlyName_SendsCorrectCommand()
    {
        SetupHttpResponse(HttpStatusCode.OK);

        var result = await _controller.SetInputAsync("Blu-ray");

        result.Should().BeTrue();
        VerifyHttpRequest("/goform/formiPhoneAppDirect.xml?SIBD");
    }

    [Fact]
    public async Task GetCurrentInputAsync_ReturnsCorrectInput()
    {
        SetupHttpPostResponse("<cmd>SIDVD</cmd>");

        var result = await _controller.GetCurrentInputAsync();

        result.Should().Be("DVD");
    }

    [Fact]
    public async Task VolumeUpAsync_SendsCorrectCommand()
    {
        SetupHttpResponse(HttpStatusCode.OK);

        var result = await _controller.VolumeUpAsync();

        result.Should().BeTrue();
        VerifyHttpRequest("/goform/formiPhoneAppDirect.xml?MVUP");
    }

    [Fact]
    public async Task VolumeDownAsync_SendsCorrectCommand()
    {
        SetupHttpResponse(HttpStatusCode.OK);

        var result = await _controller.VolumeDownAsync();

        result.Should().BeTrue();
        VerifyHttpRequest("/goform/formiPhoneAppDirect.xml?MVDOWN");
    }

    [Fact]
    public async Task GetModelInfoAsync_ParsesXmlCorrectly()
    {
        var xml = "<Device><ModelName>AVR-X3700H</ModelName></Device>";
        SetupHttpResponse(HttpStatusCode.OK, xml);

        var result = await _controller.GetModelInfoAsync();

        result.Should().Be("AVR-X3700H");
    }

    [Fact]
    public async Task GetModelInfoAsync_InvalidXml_ReturnsDefault()
    {
        SetupHttpResponse(HttpStatusCode.OK, "invalid xml");

        var result = await _controller.GetModelInfoAsync();

        result.Should().Be("Denon AVR");
    }

    [Fact]
    public async Task SetZonePowerAsync_Zone2PowerOn_SendsCorrectCommand()
    {
        SetupHttpResponse(HttpStatusCode.OK);

        var result = await _controller.SetZonePowerAsync("ZONE2", true);

        result.Should().BeTrue();
        VerifyHttpRequest("/goform/formiPhoneAppDirect.xml?Z2ON");
    }

    [Fact]
    public async Task SetZoneVolumeAsync_Zone2Volume_SendsCorrectCommand()
    {
        SetupHttpResponse(HttpStatusCode.OK);

        var result = await _controller.SetZoneVolumeAsync("2", 50);

        result.Should().BeTrue();
        VerifyHttpRequest("/goform/formiPhoneAppDirect.xml?Z240");
    }

    [Fact]
    public async Task SetZoneInputAsync_Zone3Input_SendsCorrectCommand()
    {
        SetupHttpResponse(HttpStatusCode.OK);

        var result = await _controller.SetZoneInputAsync("3", "CD");

        result.Should().BeTrue();
        VerifyHttpRequest("/goform/formiPhoneAppDirect.xml?Z3CD");
    }

    [Fact]
    public async Task SetPowerAsync_HttpError_ReturnsFalse()
    {
        SetupHttpResponse(HttpStatusCode.InternalServerError);

        var result = await _controller.SetPowerAsync(true);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task SetPowerAsync_Exception_ReturnsFalse()
    {
        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        var result = await _controller.SetPowerAsync(true);

        result.Should().BeFalse();
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

    private void SetupHttpPostResponse(string content)
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(content)
        };

        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
    }

    private void VerifyHttpRequest(string expectedPath)
    {
        _mockHttpHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Get &&
                req.RequestUri!.PathAndQuery == expectedPath),
            ItExpr.IsAny<CancellationToken>());
    }
}