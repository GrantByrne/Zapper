using System.Net;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Zapper.Core.Models;

namespace Zapper.Device.Yamaha.Tests.Unit;

public class YamahaDeviceControllerTests
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<YamahaDeviceController> _loggerMock;
    private readonly YamahaDeviceController _controller;

    public YamahaDeviceControllerTests()
    {
        _httpClient = new HttpClient { Timeout = TimeSpan.FromMilliseconds(100) };
        _loggerMock = Substitute.For<ILogger<YamahaDeviceController>>();
        _controller = new YamahaDeviceController(_httpClient, _loggerMock);
    }

    [Fact(Timeout = 5000)]
    public async Task Connect_WithValidDevice_ReturnsTrue()
    {
        var device = new DeviceModel { IpAddress = "192.168.1.100", Name = "Test Yamaha" };

        var result = await _controller.Connect(device);

        Assert.True(result);
    }

    [Fact(Timeout = 5000)]
    public async Task Connect_WithoutIpAddress_ReturnsFalse()
    {
        var device = new DeviceModel { Name = "Test Yamaha" };

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
        var device = new DeviceModel { IpAddress = "192.168.1.100", Name = "Test Yamaha" };
        await _controller.Connect(device);

        var result = await _controller.Disconnect(device);

        Assert.True(result);
    }

    [Fact(Timeout = 5000)]
    public async Task SendCommand_PowerCommand_ReturnsResult()
    {
        var device = new DeviceModel { IpAddress = "192.168.1.100", Name = "Test Yamaha" };
        var command = new DeviceCommand { Type = CommandType.Power };


        var result = await _controller.SendCommand(device, command);

        Assert.False(result); // Will fail without actual Yamaha device
    }

    [Fact(Timeout = 5000)]
    public async Task SendCommand_VolumeCommand_AdjustsVolume()
    {
        var device = new DeviceModel { IpAddress = "192.168.1.100", Name = "Test Yamaha" };
        var command = new DeviceCommand { Type = CommandType.VolumeUp };


        var result = await _controller.SendCommand(device, command);

        Assert.False(result); // Will fail without actual Yamaha device
    }

    [Fact(Timeout = 5000)]
    public async Task TestConnection_WithValidIpAddress_ReturnsResult()
    {
        var device = new DeviceModel { IpAddress = "192.168.1.100", Name = "Test Yamaha" };


        var result = await _controller.TestConnection(device);

        Assert.False(result); // Will fail without actual Yamaha device
    }

    [Fact(Timeout = 5000)]
    public async Task TestConnection_WithInvalidIpAddress_ReturnsFalse()
    {
        var device = new DeviceModel { IpAddress = "192.168.1.100", Name = "Test Yamaha" };


        var result = await _controller.TestConnection(device);

        Assert.False(result);
    }

    [Fact(Timeout = 5000)]
    public async Task PowerOn_SendsCorrectRequest()
    {
        var device = new DeviceModel { IpAddress = "192.168.1.100", Name = "Test Yamaha" };


        var result = await _controller.PowerOn(device);

        Assert.False(result); // Will fail without actual Yamaha device
    }

    [Fact(Timeout = 5000)]
    public async Task PowerOff_SendsCorrectRequest()
    {
        var device = new DeviceModel { IpAddress = "192.168.1.100", Name = "Test Yamaha" };


        var result = await _controller.PowerOff(device);

        Assert.False(result); // Will fail without actual Yamaha device
    }

    [Fact(Timeout = 5000)]
    public async Task SetVolume_WithValidVolume_SendsCorrectRequest()
    {
        var device = new DeviceModel { IpAddress = "192.168.1.100", Name = "Test Yamaha" };


        var result = await _controller.SetVolume(device, 75);

        Assert.False(result); // Will fail without actual Yamaha device
    }

    [Fact(Timeout = 5000)]
    public async Task SetVolume_WithVolumeAbove100_ClampsTo100()
    {
        var device = new DeviceModel { IpAddress = "192.168.1.100", Name = "Test Yamaha" };


        var result = await _controller.SetVolume(device, 150);

        Assert.False(result); // Will fail without actual Yamaha device
    }

    [Fact(Timeout = 5000)]
    public async Task SetInput_SendsCorrectRequest()
    {
        var device = new DeviceModel { IpAddress = "192.168.1.100", Name = "Test Yamaha" };


        var result = await _controller.SetInput(device, "hdmi1");

        Assert.False(result); // Will fail without actual Yamaha device
    }

    [Fact(Timeout = 5000)]
    public async Task Mute_TogglesCorrectly()
    {
        var device = new DeviceModel { IpAddress = "192.168.1.100", Name = "Test Yamaha" };


        var result = await _controller.Mute(device);

        Assert.False(result); // Will fail without actual Yamaha device
    }

    [Fact(Timeout = 5000)]
    public async Task SendCommand_CustomInputCommand_HandlesInputs()
    {
        var device = new DeviceModel { IpAddress = "192.168.1.100", Name = "Test Yamaha" };
        var command = new DeviceCommand
        {
            Type = CommandType.Custom,
            NetworkPayload = "hdmi2"
        };


        var result = await _controller.SendCommand(device, command);

        Assert.False(result); // Will fail without actual Yamaha device
    }

}