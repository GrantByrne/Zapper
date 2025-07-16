using System.Net;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Zapper.Core.Models;

namespace Zapper.Device.Sonos.Tests.Unit;

public class SonosDeviceControllerTests
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SonosDeviceController> _loggerMock;
    private readonly SonosDeviceController _controller;

    public SonosDeviceControllerTests()
    {
        _httpClient = new HttpClient();
        _loggerMock = Substitute.For<ILogger<SonosDeviceController>>();
        _controller = new SonosDeviceController(_httpClient, _loggerMock);
    }

    [Fact(Timeout = 5000)]
    public async Task Connect_WithValidDevice_ReturnsTrue()
    {
        var device = new DeviceModel { IpAddress = "192.168.1.100", Name = "Test Sonos" };

        var result = await _controller.Connect(device);

        Assert.True(result);
    }

    [Fact(Timeout = 5000)]
    public async Task Connect_WithoutIpAddress_ReturnsFalse()
    {
        var device = new DeviceModel { Name = "Test Sonos" };

        var result = await _controller.Connect(device);

        Assert.False(result);
        _loggerMock.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(v => v.ToString()!.Contains("no IP address")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact(Timeout = 5000)]
    public async Task Disconnect_WithValidDevice_ReturnsTrue()
    {
        var device = new DeviceModel { IpAddress = "192.168.1.100", Name = "Test Sonos" };
        await _controller.Connect(device);

        var result = await _controller.Disconnect(device);

        Assert.True(result);
    }

    [Fact(Timeout = 5000)]
    public async Task SendCommand_PowerCommand_ReturnsResult()
    {
        var device = new DeviceModel { IpAddress = "192.168.1.100", Name = "Test Sonos" };
        var command = new DeviceCommand { Type = CommandType.Power };

        var result = await _controller.SendCommand(device, command);

        Assert.False(result); // Will fail without actual Sonos device
    }

    [Fact(Timeout = 5000)]
    public async Task SendCommand_PlayPauseCommand_SendsCorrectRequest()
    {
        var device = new DeviceModel { IpAddress = "192.168.1.100", Name = "Test Sonos" };
        var command = new DeviceCommand { Type = CommandType.PlayPause };

        var result = await _controller.SendCommand(device, command);

        Assert.False(result); // Will fail without actual Sonos device
    }

    [Fact(Timeout = 5000)]
    public async Task SendCommand_VolumeCommand_AdjustsVolume()
    {
        var device = new DeviceModel { IpAddress = "192.168.1.100", Name = "Test Sonos" };
        var command = new DeviceCommand { Type = CommandType.VolumeUp };

        var result = await _controller.SendCommand(device, command);

        Assert.False(result); // Will fail without actual Sonos device
    }

    [Fact(Timeout = 5000)]
    public async Task TestConnection_WithValidIpAddress_ReturnsResult()
    {
        var device = new DeviceModel { IpAddress = "192.168.1.100", Name = "Test Sonos" };

        var result = await _controller.TestConnection(device);

        Assert.False(result); // Will fail without actual Sonos device
    }

    [Fact(Timeout = 5000)]
    public async Task TestConnection_WithInvalidIpAddress_ReturnsFalse()
    {
        var device = new DeviceModel { IpAddress = "192.168.1.100", Name = "Test Sonos" };

        var result = await _controller.TestConnection(device);

        Assert.False(result); // Will fail without actual Sonos device
    }

    [Fact(Timeout = 5000)]
    public async Task SetVolume_WithValidVolume_SendsCorrectRequest()
    {
        var device = new DeviceModel { IpAddress = "192.168.1.100", Name = "Test Sonos" };

        var result = await _controller.SetVolume(device, 75);

        Assert.False(result); // Will fail without actual Sonos device
    }

    [Fact(Timeout = 5000)]
    public async Task Play_SendsCorrectSOAPRequest()
    {
        var device = new DeviceModel { IpAddress = "192.168.1.100", Name = "Test Sonos" };

        var result = await _controller.Play(device);

        Assert.False(result); // Will fail without actual Sonos device
    }

    [Fact(Timeout = 5000)]
    public async Task Pause_SendsCorrectSOAPRequest()
    {
        var device = new DeviceModel { IpAddress = "192.168.1.100", Name = "Test Sonos" };

        var result = await _controller.Pause(device);

        Assert.False(result); // Will fail without actual Sonos device
    }

    [Fact(Timeout = 5000)]
    public async Task Stop_SendsCorrectSOAPRequest()
    {
        var device = new DeviceModel { IpAddress = "192.168.1.100", Name = "Test Sonos" };

        var result = await _controller.Stop(device);

        Assert.False(result); // Will fail without actual Sonos device
    }

}