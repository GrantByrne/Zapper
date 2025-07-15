using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Zapper.Device.USB.Tests.Unit;

public class MockUsbRemoteHandlerTests
{
    private readonly ILogger<MockUsbRemoteHandler> _logger;
    private readonly MockUsbRemoteHandler _handler;

    public MockUsbRemoteHandlerTests()
    {
        _logger = NullLogger<MockUsbRemoteHandler>.Instance;
        _handler = new MockUsbRemoteHandler(_logger);
    }

    [Fact]
    public void IsListening_WhenNotStarted_ShouldReturnFalse()
    {
        _handler.IsListening.Should().BeFalse();
    }

    [Fact]
    public async Task StartListeningAsync_ShouldSetIsListeningToTrue()
    {
        await _handler.StartListeningAsync();

        _handler.IsListening.Should().BeTrue();
    }

    [Fact]
    public async Task StartListeningAsync_WhenAlreadyListening_ShouldNotThrow()
    {
        await _handler.StartListeningAsync();

        var act = async () => await _handler.StartListeningAsync();

        await act.Should().NotThrowAsync();
        _handler.IsListening.Should().BeTrue();
    }

    [Fact]
    public async Task StopListeningAsync_ShouldSetIsListeningToFalse()
    {
        await _handler.StartListeningAsync();
        await _handler.StopListeningAsync();

        _handler.IsListening.Should().BeFalse();
    }

    [Fact]
    public async Task StopListeningAsync_WhenNotListening_ShouldNotThrow()
    {
        var act = async () => await _handler.StopListeningAsync();

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public void GetConnectedRemotes_ShouldReturnMockDevices()
    {
        var remotes = _handler.GetConnectedRemotes();

        remotes.Should().NotBeEmpty();
        remotes.Should().Contain("MOCK:0001:remote1");
        remotes.Should().Contain("MOCK:0002:remote2");
    }

    [Fact]
    public async Task SimulateButtonPress_WhenListening_ShouldRaiseButtonPressedEvent()
    {
        await _handler.StartListeningAsync();

        RemoteButtonEventArgs? capturedArgs = null;
        _handler.ButtonPressed += (sender, args) => capturedArgs = args;

        _handler.SimulateButtonPress("TEST:001", "Power", 0x01);

        capturedArgs.Should().NotBeNull();
        capturedArgs!.DeviceId.Should().Be("TEST:001");
        capturedArgs.ButtonName.Should().Be("Power");
        capturedArgs.KeyCode.Should().Be(0x01);
        capturedArgs.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void SimulateButtonPress_WhenNotListening_ShouldNotRaiseEvent()
    {
        var eventRaised = false;
        _handler.ButtonPressed += (sender, args) => eventRaised = true;

        _handler.SimulateButtonPress("TEST:001", "Power", 0x01);

        eventRaised.Should().BeFalse();
    }

    [Fact]
    public async Task ButtonPressedEvent_ShouldContainCorrectTimestamp()
    {
        await _handler.StartListeningAsync();

        var before = DateTime.UtcNow;
        RemoteButtonEventArgs? capturedArgs = null;
        _handler.ButtonPressed += (sender, args) => capturedArgs = args;

        _handler.SimulateButtonPress("TEST:001", "VolumeUp", 0x02);
        var after = DateTime.UtcNow;

        capturedArgs.Should().NotBeNull();
        capturedArgs!.Timestamp.Should().BeAfter(before).And.BeBefore(after);
    }

    [Fact]
    public async Task MultipleEventSubscribers_ShouldAllReceiveEvents()
    {
        await _handler.StartListeningAsync();

        var subscriber1Called = false;
        var subscriber2Called = false;

        _handler.ButtonPressed += (sender, args) => subscriber1Called = true;
        _handler.ButtonPressed += (sender, args) => subscriber2Called = true;

        _handler.SimulateButtonPress("TEST:001", "Menu", 0x0C);

        subscriber1Called.Should().BeTrue();
        subscriber2Called.Should().BeTrue();
    }

    [Fact]
    public async Task EventUnsubscription_ShouldPreventEventDelivery()
    {
        await _handler.StartListeningAsync();

        var eventReceived = false;
        EventHandler<RemoteButtonEventArgs> handler = (sender, args) => eventReceived = true;

        _handler.ButtonPressed += handler;
        _handler.ButtonPressed -= handler;

        _handler.SimulateButtonPress("TEST:001", "Back", 0x0D);

        eventReceived.Should().BeFalse();
    }
}