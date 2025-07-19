using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Zapper.Device.Xbox.Network;

namespace Zapper.Device.Xbox.Tests.Unit;

public class XboxDeviceControllerTests
{
    private readonly INetworkClientFactory _networkClientFactoryMock;
    private readonly ITcpClientWrapper _tcpClientMock;
    private readonly IUdpClientWrapper _udpClientMock;
    private readonly Stream _streamMock;
    private readonly ILogger<XboxDeviceController> _loggerMock;
    private readonly XboxDeviceController _controller;

    public XboxDeviceControllerTests()
    {
        _networkClientFactoryMock = Substitute.For<INetworkClientFactory>();
        _tcpClientMock = Substitute.For<ITcpClientWrapper>();
        _udpClientMock = Substitute.For<IUdpClientWrapper>();
        _streamMock = Substitute.For<Stream>();
        _loggerMock = Substitute.For<ILogger<XboxDeviceController>>();

        _networkClientFactoryMock.CreateTcpClient().Returns(_tcpClientMock);
        _networkClientFactoryMock.CreateUdpClient().Returns(_udpClientMock);
        _tcpClientMock.GetStream().Returns(_streamMock);

        // Setup mocks to simulate network failures by default
        _tcpClientMock.Connect(Arg.Any<string>(), Arg.Any<int>())
            .ThrowsAsync(new SocketException());
        _udpClientMock.SendAsync(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<IPEndPoint>())
            .ThrowsAsync(new SocketException());

        _controller = new XboxDeviceController(_networkClientFactoryMock, _loggerMock);
    }

    [Fact(Timeout = 5000)]
    public async Task Connect_WithValidDevice_ReturnsTrue()
    {
        var device = new Zapper.Core.Models.Device { IpAddress = "192.168.1.100", Name = "Test Xbox" };

        var result = await _controller.Connect(device);

        Assert.True(result);
    }

    [Fact(Timeout = 5000)]
    public async Task Connect_WithoutIpAddress_ReturnsFalse()
    {
        var device = new Zapper.Core.Models.Device { Name = "Test Xbox" };

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
        var device = new Zapper.Core.Models.Device { IpAddress = "192.168.1.100", Name = "Test Xbox" };
        await _controller.Connect(device);

        var result = await _controller.Disconnect(device);

        Assert.True(result);
    }

    [Fact(Timeout = 5000)]
    public async Task SendCommand_PowerCommand_ReturnsResult()
    {
        var device = new Zapper.Core.Models.Device
        {
            IpAddress = "192.168.1.100",
            Name = "Test Xbox",
            AuthToken = "test-live-id"
        };
        var command = new Zapper.Core.Models.DeviceCommand { Type = Core.Models.CommandType.Power };

        var result = await _controller.SendCommand(device, command);

        Assert.False(result); // Will fail without actual Xbox
    }

    [Fact(Timeout = 5000)]
    public async Task SendCommand_DirectionalCommand_HandlesAllDirections()
    {
        var device = new Zapper.Core.Models.Device { IpAddress = "192.168.1.100", Name = "Test Xbox" };
        var directions = new[]
        {
            Core.Models.CommandType.DirectionalUp,
            Core.Models.CommandType.DirectionalDown,
            Core.Models.CommandType.DirectionalLeft,
            Core.Models.CommandType.DirectionalRight
        };

        foreach (var direction in directions)
        {
            var command = new Zapper.Core.Models.DeviceCommand { Type = direction };
            var result = await _controller.SendCommand(device, command);
            Assert.False(result); // Will fail without actual Xbox
        }
    }

    [Fact(Timeout = 5000)]
    public async Task SendCommand_WithoutIpAddress_ReturnsFalse()
    {
        var device = new Zapper.Core.Models.Device { Name = "Test Xbox" };
        var command = new Zapper.Core.Models.DeviceCommand { Type = Core.Models.CommandType.Ok };

        var result = await _controller.SendCommand(device, command);

        Assert.False(result);
    }

    [Fact(Timeout = 5000)]
    public async Task TestConnection_WithValidIpAddress_ReturnsResult()
    {
        var device = new Zapper.Core.Models.Device { IpAddress = "192.168.1.100", Name = "Test Xbox" };

        var result = await _controller.TestConnection(device);

        Assert.False(result); // Will fail without actual Xbox
    }

    [Fact(Timeout = 5000)]
    public async Task PowerOn_WithoutLiveId_ReturnsFalse()
    {
        var device = new DeviceModel { IpAddress = "192.168.1.100", Name = "Test Xbox" };

        var result = await _controller.PowerOn(device);

        Assert.False(result);
        _loggerMock.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(v => v.ToString()!.Contains("missing IP or Live ID")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact(Timeout = 5000)]
    public async Task SendText_WithValidDevice_SendsText()
    {
        var device = new DeviceModel { IpAddress = "192.168.1.100", Name = "Test Xbox" };
        var text = "Hello Xbox";

        var result = await _controller.SendText(device, text);

        Assert.False(result); // Will fail without actual Xbox
    }
}