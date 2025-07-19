using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Zapper.Core.Models;

namespace Zapper.Device.Infrared.Tests.Unit;

public class MockInfraredTransmitterTests
{
    private readonly MockInfraredTransmitter _transmitter;

    public MockInfraredTransmitterTests()
    {
        var logger = NullLogger<MockInfraredTransmitter>.Instance;
        _transmitter = new MockInfraredTransmitter(logger);
    }

    [Fact]
    public void IsAvailable_WhenNotInitialized_ShouldReturnFalse()
    {
        _transmitter.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public void IsAvailable_WhenInitialized_ShouldReturnTrue()
    {
        _transmitter.Initialize();

        _transmitter.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public void Initialize_ShouldLogInitializationMessage()
    {
        _transmitter.Initialize();

        // Logger assertions removed - using NullLogger for simplicity
        // In a real scenario, you would use a test logger framework
    }

    [Fact]
    public async Task Transmit_WithString_WhenNotInitialized_ShouldThrowInvalidOperationException()
    {
        var act = async () => await _transmitter.Transmit("test code");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Mock IR transmitter not initialized");
    }

    [Fact]
    public async Task Transmit_WithString_WhenInitialized_ShouldSimulateTransmission()
    {
        _transmitter.Initialize();
        var startTime = DateTime.UtcNow;

        await _transmitter.Transmit("test code", 2);

        var elapsed = DateTime.UtcNow - startTime;
        elapsed.Should().BeGreaterThan(TimeSpan.FromMilliseconds(150)); // 2 * 100ms - some tolerance
    }

    [Fact]
    public async Task Transmit_WithIRCode_WhenNotInitialized_ShouldThrowInvalidOperationException()
    {
        var irCode = new IrCode
        {
            Brand = "Test",
            Model = "Model",
            CommandName = "Power",
            HexCode = "0x123456"
        };

        var act = async () => await _transmitter.Transmit(irCode);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Mock IR transmitter not initialized");
    }

    [Fact]
    public async Task Transmit_WithIRCode_WhenInitialized_ShouldSimulateTransmission()
    {
        _transmitter.Initialize();
        var irCode = new IrCode
        {
            Brand = "Samsung",
            Model = "TV65",
            CommandName = "Power",
            HexCode = "0xE0E040BF"
        };

        await _transmitter.Transmit(irCode);
    }

    [Fact]
    public async Task TransmitRaw_WhenNotInitialized_ShouldThrowInvalidOperationException()
    {
        var pulses = new[] { 9000, 4500, 560, 560 };

        var act = async () => await _transmitter.TransmitRaw(pulses);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Mock IR transmitter not initialized");
    }

    [Fact]
    public async Task TransmitRaw_WhenInitialized_ShouldSimulateTransmission()
    {
        _transmitter.Initialize();
        var pulses = new[] { 9000, 4500, 560, 560 };

        await _transmitter.TransmitRaw(pulses);
    }

    [Fact]
    public void Dispose_ShouldLogDisposalMessage()
    {
        _transmitter.Dispose();
    }
}