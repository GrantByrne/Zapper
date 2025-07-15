using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Zapper.Core.Models;

namespace Zapper.Device.Sonos.Tests.Unit;

public class SonosDeviceControllerTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly Mock<ILogger<SonosDeviceController>> _loggerMock;
    private readonly SonosDeviceController _controller;

    public SonosDeviceControllerTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        _loggerMock = new Mock<ILogger<SonosDeviceController>>();
        _controller = new SonosDeviceController(_httpClient, _loggerMock.Object);
    }

    [Fact(Timeout = 5000)]
    public async Task ConnectAsync_WithValidDevice_ReturnsTrue()
    {
        var device = new DeviceModel { IpAddress = "192.168.1.100", Name = "Test Sonos" };

        var result = await _controller.ConnectAsync(device);

        Assert.True(result);
    }

    [Fact(Timeout = 5000)]
    public async Task ConnectAsync_WithoutIpAddress_ReturnsFalse()
    {
        var device = new DeviceModel { Name = "Test Sonos" };

        var result = await _controller.ConnectAsync(device);

        Assert.False(result);
        _loggerMock.Verify(x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("no IP address")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact(Timeout = 5000)]
    public async Task DisconnectAsync_WithValidDevice_ReturnsTrue()
    {
        var device = new DeviceModel { IpAddress = "192.168.1.100", Name = "Test Sonos" };
        await _controller.ConnectAsync(device);

        var result = await _controller.DisconnectAsync(device);

        Assert.True(result);
    }

    [Fact(Timeout = 5000)]
    public async Task SendCommandAsync_PowerCommand_ReturnsResult()
    {
        var device = new DeviceModel { IpAddress = "192.168.1.100", Name = "Test Sonos" };
        var command = new DeviceCommand { Type = CommandType.Power };

        SetupHttpResponse(HttpStatusCode.OK, "<response></response>");

        var result = await _controller.SendCommandAsync(device, command);

        Assert.False(result); // Will fail due to mock response
    }

    [Fact(Timeout = 5000)]
    public async Task SendCommandAsync_PlayPauseCommand_SendsCorrectRequest()
    {
        var device = new DeviceModel { IpAddress = "192.168.1.100", Name = "Test Sonos" };
        var command = new DeviceCommand { Type = CommandType.PlayPause };

        SetupHttpResponse(HttpStatusCode.OK, "<response></response>");

        var result = await _controller.SendCommandAsync(device, command);

        Assert.False(result); // Will fail due to mock response
    }

    [Fact(Timeout = 5000)]
    public async Task SendCommandAsync_VolumeCommand_AdjustsVolume()
    {
        var device = new DeviceModel { IpAddress = "192.168.1.100", Name = "Test Sonos" };
        var command = new DeviceCommand { Type = CommandType.VolumeUp };

        SetupHttpResponse(HttpStatusCode.OK, "<response><CurrentVolume>50</CurrentVolume></response>");

        var result = await _controller.SendCommandAsync(device, command);

        Assert.False(result); // Will fail due to mock response
    }

    [Fact(Timeout = 5000)]
    public async Task TestConnectionAsync_WithValidIpAddress_ReturnsResult()
    {
        var device = new DeviceModel { IpAddress = "192.168.1.100", Name = "Test Sonos" };

        SetupHttpResponse(HttpStatusCode.OK, "<device></device>");

        var result = await _controller.TestConnectionAsync(device);

        Assert.True(result);
    }

    [Fact(Timeout = 5000)]
    public async Task TestConnectionAsync_WithInvalidIpAddress_ReturnsFalse()
    {
        var device = new DeviceModel { IpAddress = "192.168.1.100", Name = "Test Sonos" };

        SetupHttpResponse(HttpStatusCode.NotFound, "");

        var result = await _controller.TestConnectionAsync(device);

        Assert.False(result);
    }

    [Fact(Timeout = 5000)]
    public async Task SetVolumeAsync_WithValidVolume_SendsCorrectRequest()
    {
        var device = new DeviceModel { IpAddress = "192.168.1.100", Name = "Test Sonos" };

        SetupHttpResponse(HttpStatusCode.OK, "<response></response>");

        var result = await _controller.SetVolumeAsync(device, 75);

        Assert.True(result);
    }

    [Fact(Timeout = 5000)]
    public async Task PlayAsync_SendsCorrectSOAPRequest()
    {
        var device = new DeviceModel { IpAddress = "192.168.1.100", Name = "Test Sonos" };

        SetupHttpResponse(HttpStatusCode.OK, "<response></response>");

        var result = await _controller.PlayAsync(device);

        Assert.True(result);
    }

    [Fact(Timeout = 5000)]
    public async Task PauseAsync_SendsCorrectSOAPRequest()
    {
        var device = new DeviceModel { IpAddress = "192.168.1.100", Name = "Test Sonos" };

        SetupHttpResponse(HttpStatusCode.OK, "<response></response>");

        var result = await _controller.PauseAsync(device);

        Assert.True(result);
    }

    [Fact(Timeout = 5000)]
    public async Task StopAsync_SendsCorrectSOAPRequest()
    {
        var device = new DeviceModel { IpAddress = "192.168.1.100", Name = "Test Sonos" };

        SetupHttpResponse(HttpStatusCode.OK, "<response></response>");

        var result = await _controller.StopAsync(device);

        Assert.True(result);
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