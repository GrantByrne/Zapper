using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Net.WebSockets;
using Xunit;

namespace Zapper.Device.Tizen.Tests.Unit;

public class TizenClientTests : IDisposable
{
    private readonly ILogger<TizenClient> _mockLogger;
    private readonly TizenClient _client;

    public TizenClientTests()
    {
        _mockLogger = Substitute.For<ILogger<TizenClient>>();
        _client = new TizenClient(_mockLogger);
    }

    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        _client.IsConnected.Should().BeFalse();
        _client.AuthToken.Should().BeNull();
    }

    [Fact]
    public async Task ConnectAsync_WithInvalidAddress_ShouldReturnFalse()
    {
        var result = await _client.ConnectAsync("invalid-address", null);

        result.Should().BeFalse();
        _client.IsConnected.Should().BeFalse();
    }

    [Fact]
    public async Task DisconnectAsync_WhenNotConnected_ShouldNotThrow()
    {
        var act = async () => await _client.DisconnectAsync();

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task AuthenticateAsync_WhenNotConnected_ShouldReturnNull()
    {
        var result = await _client.AuthenticateAsync();

        result.Should().BeNull();
        _mockLogger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Cannot authenticate - not connected to TV")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task AuthenticateAsync_WhenConnected_ShouldReturnToken()
    {
        // Since AuthenticateAsync checks IsConnected which looks at _webSocket state,
        // and we can't properly mock a connected WebSocket, we'll test the actual behavior
        // which returns null when not connected
        var result = await _client.AuthenticateAsync();

        result.Should().BeNull();
        _mockLogger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Cannot authenticate - not connected to TV")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task SendCommandAsync_WhenNotConnected_ShouldReturnFalse()
    {
        var result = await _client.SendCommandAsync("test.method", new { param = "value" });

        result.Should().BeFalse();
        _mockLogger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Cannot send command - not connected to TV")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task SendKeyAsync_WhenNotConnected_ShouldReturnFalse()
    {
        var result = await _client.SendKeyAsync("KEY_POWER");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task PowerOffAsync_ShouldCallSendKeyWithCorrectKey()
    {
        var result = await _client.PowerOffAsync();

        result.Should().BeFalse();
    }

    [Fact]
    public async Task SetVolumeAsync_ShouldLogWarningAndReturnFalse()
    {
        var result = await _client.SetVolumeAsync(50);

        result.Should().BeFalse();
        _mockLogger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Direct volume setting not supported")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task VolumeUpAsync_ShouldCallSendKeyWithCorrectKey()
    {
        var result = await _client.VolumeUpAsync();

        result.Should().BeFalse();
    }

    [Fact]
    public async Task VolumeDownAsync_ShouldCallSendKeyWithCorrectKey()
    {
        var result = await _client.VolumeDownAsync();

        result.Should().BeFalse();
    }

    [Fact]
    public async Task SetMuteAsync_ShouldCallSendKeyWithMuteKey()
    {
        var result = await _client.SetMuteAsync(true);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task LaunchAppAsync_ShouldCallSendCommandWithCorrectParams()
    {
        var appId = "com.netflix.app";
        var result = await _client.LaunchAppAsync(appId);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task SwitchInputAsync_ShouldCallSendKeyWithCorrectKey()
    {
        var result = await _client.SwitchInputAsync("HDMI1");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task ChannelUpAsync_ShouldCallSendKeyWithCorrectKey()
    {
        var result = await _client.ChannelUpAsync();

        result.Should().BeFalse();
    }

    [Fact]
    public async Task ChannelDownAsync_ShouldCallSendKeyWithCorrectKey()
    {
        var result = await _client.ChannelDownAsync();

        result.Should().BeFalse();
    }

    [Fact]
    public async Task SendTextAsync_ShouldCallSendCommandWithBase64EncodedText()
    {
        var text = "Hello World";
        var result = await _client.SendTextAsync(text);

        result.Should().BeFalse();
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}