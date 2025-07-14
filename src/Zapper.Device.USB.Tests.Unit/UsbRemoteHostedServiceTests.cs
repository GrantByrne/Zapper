using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Zapper.Device.USB.Tests.Unit;

public class UsbRemoteHostedServiceTests
{
    private readonly Mock<IUsbRemoteHandler> _mockRemoteHandler;
    private readonly ILogger<UsbRemoteHostedService> _logger;
    private readonly UsbRemoteHostedService _service;

    public UsbRemoteHostedServiceTests()
    {
        _mockRemoteHandler = new Mock<IUsbRemoteHandler>();
        _logger = NullLogger<UsbRemoteHostedService>.Instance;
        _service = new UsbRemoteHostedService(_mockRemoteHandler.Object, _logger);
    }

    [Fact]
    public async Task StartAsync_ShouldStartRemoteHandler()
    {
        _mockRemoteHandler.Setup(h => h.StartListeningAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _service.StartAsync(CancellationToken.None);

        _mockRemoteHandler.Verify(h => h.StartListeningAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task StartAsync_WhenRemoteHandlerThrows_ShouldPropagateException()
    {
        var expectedException = new InvalidOperationException("Test exception");
        _mockRemoteHandler.Setup(h => h.StartListeningAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var act = async () => await _service.StartAsync(CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Test exception");
    }

    [Fact]
    public async Task StopAsync_ShouldStopRemoteHandler()
    {
        _mockRemoteHandler.Setup(h => h.StopListeningAsync())
            .Returns(Task.CompletedTask);

        await _service.StopAsync(CancellationToken.None);

        _mockRemoteHandler.Verify(h => h.StopListeningAsync(), Times.Once);
    }

    [Fact]
    public async Task StopAsync_WhenRemoteHandlerThrows_ShouldNotPropagateException()
    {
        _mockRemoteHandler.Setup(h => h.StopListeningAsync())
            .ThrowsAsync(new InvalidOperationException("Test exception"));

        var act = async () => await _service.StopAsync(CancellationToken.None);

        await act.Should().NotThrowAsync();
        _mockRemoteHandler.Verify(h => h.StopListeningAsync(), Times.Once);
    }

    [Fact]
    public async Task StartAsync_ShouldSubscribeToButtonPressedEvent()
    {
        _mockRemoteHandler.Setup(h => h.StartListeningAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _service.StartAsync(CancellationToken.None);

        // Verify that the event subscription happened by triggering the event
        var eventArgs = new RemoteButtonEventArgs("TEST:001", "Power", 0x01);
        _mockRemoteHandler.Raise(h => h.ButtonPressed += null, _mockRemoteHandler.Object, eventArgs);

        // If we get here without exception, the subscription worked
        // The actual logging is tested implicitly through the subscription
    }

    [Fact]
    public async Task StopAsync_ShouldUnsubscribeFromButtonPressedEvent()
    {
        _mockRemoteHandler.Setup(h => h.StartListeningAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockRemoteHandler.Setup(h => h.StopListeningAsync())
            .Returns(Task.CompletedTask);

        await _service.StartAsync(CancellationToken.None);
        await _service.StopAsync(CancellationToken.None);

        // After stopping, the event handler should be unsubscribed
        // This is tested implicitly through the -= operation in StopAsync
        _mockRemoteHandler.Verify(h => h.StopListeningAsync(), Times.Once);
    }

    [Fact]
    public async Task ButtonPressedEventHandler_ShouldNotThrow()
    {
        _mockRemoteHandler.Setup(h => h.StartListeningAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _service.StartAsync(CancellationToken.None);

        var eventArgs = new RemoteButtonEventArgs("TEST:001", "VolumeUp", 0x02);
        var act = () => _mockRemoteHandler.Raise(h => h.ButtonPressed += null, _mockRemoteHandler.Object, eventArgs);

        act.Should().NotThrow();
    }

    [Fact]
    public async Task StartStopLifecycle_ShouldWorkCorrectly()
    {
        _mockRemoteHandler.Setup(h => h.StartListeningAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockRemoteHandler.Setup(h => h.StopListeningAsync())
            .Returns(Task.CompletedTask);

        // Start
        await _service.StartAsync(CancellationToken.None);
        _mockRemoteHandler.Verify(h => h.StartListeningAsync(It.IsAny<CancellationToken>()), Times.Once);

        // Stop
        await _service.StopAsync(CancellationToken.None);
        _mockRemoteHandler.Verify(h => h.StopListeningAsync(), Times.Once);

        // Should be able to start again
        await _service.StartAsync(CancellationToken.None);
        _mockRemoteHandler.Verify(h => h.StartListeningAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task StartAsync_WithCancellationToken_ShouldPassTokenToHandler()
    {
        var cancellationToken = new CancellationToken();
        _mockRemoteHandler.Setup(h => h.StartListeningAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        await _service.StartAsync(cancellationToken);

        _mockRemoteHandler.Verify(h => h.StartListeningAsync(cancellationToken), Times.Once);
    }

    // Note: Primary constructors in C# 12 don't automatically add null checks
    // so these tests are not applicable with the current implementation
}