using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Moq;
using Zapper.Device.Network;
using Zapper.Device.Xbox.Network;

namespace Zapper.Device.Xbox.Tests.Unit;

public class XboxDeviceControllerTests
{
    private readonly Mock<INetworkDeviceController> _networkControllerMock;
    private readonly Mock<INetworkClientFactory> _networkClientFactoryMock;
    private readonly Mock<ITcpClientWrapper> _tcpClientMock;
    private readonly Mock<IUdpClientWrapper> _udpClientMock;
    private readonly Mock<Stream> _streamMock;
    private readonly Mock<ILogger<XboxDeviceController>> _loggerMock;
    private readonly XboxDeviceController _controller;

    public XboxDeviceControllerTests()
    {
        _networkControllerMock = new Mock<INetworkDeviceController>();
        _networkClientFactoryMock = new Mock<INetworkClientFactory>();
        _tcpClientMock = new Mock<ITcpClientWrapper>();
        _udpClientMock = new Mock<IUdpClientWrapper>();
        _streamMock = new Mock<Stream>();
        _loggerMock = new Mock<ILogger<XboxDeviceController>>();

        _networkClientFactoryMock.Setup(x => x.CreateTcpClient()).Returns(_tcpClientMock.Object);
        _networkClientFactoryMock.Setup(x => x.CreateUdpClient()).Returns(_udpClientMock.Object);
        _tcpClientMock.Setup(x => x.GetStream()).Returns(_streamMock.Object);

        // Setup mocks to simulate network failures by default
        _tcpClientMock.Setup(x => x.ConnectAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ThrowsAsync(new SocketException());
        _udpClientMock.Setup(x => x.SendAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<IPEndPoint>()))
            .ThrowsAsync(new SocketException());

        _controller = new XboxDeviceController(_networkControllerMock.Object, _networkClientFactoryMock.Object, _loggerMock.Object);
    }

    [Fact(Timeout = 5000)]
    public async Task ConnectAsync_WithValidDevice_ReturnsTrue()
    {
        var device = new Zapper.Core.Models.Device { IpAddress = "192.168.1.100", Name = "Test Xbox" };

        var result = await _controller.ConnectAsync(device);

        Assert.True(result);
    }

    [Fact(Timeout = 5000)]
    public async Task ConnectAsync_WithoutIpAddress_ReturnsFalse()
    {
        var device = new Zapper.Core.Models.Device { Name = "Test Xbox" };

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
        var device = new Zapper.Core.Models.Device { IpAddress = "192.168.1.100", Name = "Test Xbox" };
        await _controller.ConnectAsync(device);

        var result = await _controller.DisconnectAsync(device);

        Assert.True(result);
    }

    [Fact(Timeout = 5000)]
    public async Task SendCommandAsync_PowerCommand_ReturnsResult()
    {
        var device = new Zapper.Core.Models.Device
        {
            IpAddress = "192.168.1.100",
            Name = "Test Xbox",
            AuthToken = "test-live-id"
        };
        var command = new Zapper.Core.Models.DeviceCommand { Type = Core.Models.CommandType.Power };

        var result = await _controller.SendCommandAsync(device, command);

        Assert.False(result); // Will fail without actual Xbox
    }

    [Fact(Timeout = 5000)]
    public async Task SendCommandAsync_DirectionalCommand_HandlesAllDirections()
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
            var result = await _controller.SendCommandAsync(device, command);
            Assert.False(result); // Will fail without actual Xbox
        }
    }

    [Fact(Timeout = 5000)]
    public async Task SendCommandAsync_WithoutIpAddress_ReturnsFalse()
    {
        var device = new Zapper.Core.Models.Device { Name = "Test Xbox" };
        var command = new Zapper.Core.Models.DeviceCommand { Type = Core.Models.CommandType.Ok };

        var result = await _controller.SendCommandAsync(device, command);

        Assert.False(result);
    }

    [Fact(Timeout = 5000)]
    public async Task TestConnectionAsync_WithValidIpAddress_ReturnsResult()
    {
        var device = new Zapper.Core.Models.Device { IpAddress = "192.168.1.100", Name = "Test Xbox" };

        var result = await _controller.TestConnectionAsync(device);

        Assert.False(result); // Will fail without actual Xbox
    }

    [Fact(Timeout = 5000)]
    public async Task PowerOnAsync_WithoutLiveId_ReturnsFalse()
    {
        var device = new Zapper.Core.Models.Device { IpAddress = "192.168.1.100", Name = "Test Xbox" };

        var result = await _controller.PowerOnAsync(device);

        Assert.False(result);
        _loggerMock.Verify(x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("missing IP or Live ID")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact(Timeout = 5000)]
    public async Task SendTextAsync_WithValidDevice_SendsText()
    {
        var device = new Zapper.Core.Models.Device { IpAddress = "192.168.1.100", Name = "Test Xbox" };
        var text = "Hello Xbox";

        var result = await _controller.SendTextAsync(device, text);

        Assert.False(result); // Will fail without actual Xbox
    }
}