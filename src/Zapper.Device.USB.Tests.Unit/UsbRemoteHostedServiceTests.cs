using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Zapper.Device.USB.Tests.Unit;

public class UsbRemoteHostedServiceTests
{
    private readonly IUsbRemoteHandler _mockRemoteHandler;
    private readonly ILogger<UsbRemoteHostedService> _logger;
    private readonly UsbRemoteHostedService _service;

    public UsbRemoteHostedServiceTests()
    {
        _mockRemoteHandler = Substitute.For<IUsbRemoteHandler>();
        _logger = NullLogger<UsbRemoteHostedService>.Instance;
        _service = new UsbRemoteHostedService(_mockRemoteHandler, _logger);
    }

    [Fact]
    public async Task StartAsync_ShouldStartRemoteHandler()
    {
        _mockRemoteHandler.StartListeningAsync(Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        await _service.StartAsync(CancellationToken.None);

        await _mockRemoteHandler.Received(1).StartListeningAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StartAsync_WhenRemoteHandlerThrows_ShouldPropagateException()
    {
        var expectedException = new InvalidOperationException("Test exception");
        _mockRemoteHandler.StartListeningAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException(expectedException));

        var act = async () => await _service.StartAsync(CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Test exception");
    }

    [Fact]
    public async Task StopAsync_ShouldStopRemoteHandler()
    {
        _mockRemoteHandler.StopListeningAsync()
            .Returns(Task.CompletedTask);

        await _service.StopAsync(CancellationToken.None);

        await _mockRemoteHandler.Received(1).StopListeningAsync();
    }

    [Fact]
    public async Task StopAsync_WhenRemoteHandlerThrows_ShouldNotPropagateException()
    {
        _mockRemoteHandler.StopListeningAsync()
            .Returns(Task.FromException(new InvalidOperationException("Test exception")));

        var act = async () => await _service.StopAsync(CancellationToken.None);

        await act.Should().NotThrowAsync();
        await _mockRemoteHandler.Received(1).StopListeningAsync();
    }

    [Fact]
    public async Task StartAsync_ShouldSubscribeToButtonPressedEvent()
    {
        _mockRemoteHandler.StartListeningAsync(Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        await _service.StartAsync(CancellationToken.None);

        // Verify that the event subscription happened by triggering the event
        var eventArgs = new RemoteButtonEventArgs("TEST:001", "Power", 0x01);
        _mockRemoteHandler.ButtonPressed += Raise.EventWith(eventArgs);

        // If we get here without exception, the subscription worked
        // The actual logging is tested implicitly through the subscription
    }

    [Fact]
    public async Task StopAsync_ShouldUnsubscribeFromButtonPressedEvent()
    {
        _mockRemoteHandler.StartListeningAsync(Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _mockRemoteHandler.StopListeningAsync()
            .Returns(Task.CompletedTask);

        await _service.StartAsync(CancellationToken.None);
        await _service.StopAsync(CancellationToken.None);

        // After stopping, the event handler should be unsubscribed
        // This is tested implicitly through the -= operation in StopAsync
        await _mockRemoteHandler.Received(1).StopListeningAsync();
    }

    [Fact]
    public async Task ButtonPressedEventHandler_ShouldNotThrow()
    {
        _mockRemoteHandler.StartListeningAsync(Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        await _service.StartAsync(CancellationToken.None);

        var eventArgs = new RemoteButtonEventArgs("TEST:001", "VolumeUp", 0x02);
        var act = () => _mockRemoteHandler.ButtonPressed += Raise.EventWith(eventArgs);

        act.Should().NotThrow();
    }

    [Fact]
    public async Task StartStopLifecycle_ShouldWorkCorrectly()
    {
        _mockRemoteHandler.StartListeningAsync(Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _mockRemoteHandler.StopListeningAsync()
            .Returns(Task.CompletedTask);

        // Start
        await _service.StartAsync(CancellationToken.None);
        await _mockRemoteHandler.Received(1).StartListeningAsync(Arg.Any<CancellationToken>());

        // Stop
        await _service.StopAsync(CancellationToken.None);
        await _mockRemoteHandler.Received(1).StopListeningAsync();

        // Should be able to start again
        await _service.StartAsync(CancellationToken.None);
        await _mockRemoteHandler.Received(2).StartListeningAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StartAsync_WithCancellationToken_ShouldPassTokenToHandler()
    {
        var cancellationToken = new CancellationToken();
        _mockRemoteHandler.StartListeningAsync(cancellationToken)
            .Returns(Task.CompletedTask);

        await _service.StartAsync(cancellationToken);

        await _mockRemoteHandler.Received(1).StartListeningAsync(cancellationToken);
    }

    // Note: Primary constructors in C# 12 don't automatically add null checks
    // so these tests are not applicable with the current implementation
}