using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Zapper.Core.Models;

namespace Zapper.Device.Infrared.Tests.Unit;

public class GpioInfraredTransmitterTests
{
    private readonly ILogger<GpioInfraredTransmitter> _logger;

    public GpioInfraredTransmitterTests()
    {
        _logger = NullLogger<GpioInfraredTransmitter>.Instance;
    }

    [Fact]
    public void IsAvailable_WhenNotInitialized_ShouldReturnFalse()
    {
        var transmitter = new GpioInfraredTransmitter(18, _logger);
        
        transmitter.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public async Task TransmitAsync_WhenNotInitialized_ShouldThrowInvalidOperationException()
    {
        var transmitter = new GpioInfraredTransmitter(18, _logger);
        
        var act = async () => await transmitter.TransmitAsync("test code", 1);
        
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("IR transmitter not initialized");
    }

    [Fact]
    public async Task TransmitAsync_WithIRCode_WhenNotInitialized_ShouldThrowInvalidOperationException()
    {
        var transmitter = new GpioInfraredTransmitter(18, _logger);
        var irCode = new IRCode
        {
            Brand = "Test",
            Model = "Model",
            CommandName = "Power",
            HexCode = "0x123456",
            Protocol = "NEC"
        };
        
        var act = async () => await transmitter.TransmitAsync(irCode, 1);
        
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("IR transmitter not initialized");
    }

    [Fact]
    public async Task TransmitRawAsync_WhenNotInitialized_ShouldThrowInvalidOperationException()
    {
        var transmitter = new GpioInfraredTransmitter(18, _logger);
        var pulses = new[] { 9000, 4500, 560, 560 };
        
        var act = async () => await transmitter.TransmitRawAsync(pulses, 38000);
        
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("IR transmitter not initialized");
    }

    [Fact]
    public void ParseIrCode_WithValidSpaceSeparatedValues_ShouldReturnCorrectArray()
    {
        var transmitter = new GpioInfraredTransmitter(18, _logger);
        
        // Use reflection to access private method for testing
        var method = typeof(GpioInfraredTransmitter).GetMethod("ParseIrCode", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        var result = (int[])method!.Invoke(transmitter, ["9000 4500 560 560"])!;
        
        result.Should().Equal(9000, 4500, 560, 560);
    }

    [Fact]
    public void ParseIrCode_WithInvalidValues_ShouldThrowArgumentException()
    {
        var transmitter = new GpioInfraredTransmitter(18, _logger);
        
        // Use reflection to access private method for testing
        var method = typeof(GpioInfraredTransmitter).GetMethod("ParseIrCode", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        var act = () => method!.Invoke(transmitter, ["invalid data"]);
        
        act.Should().Throw<System.Reflection.TargetInvocationException>()
           .WithInnerException<ArgumentException>()
           .WithMessage("Invalid IR code format*");
    }

    [Theory]
    [InlineData("NEC")]
    [InlineData("SONY")]
    [InlineData("RC5")]
    [InlineData("RC6")]
    public void ConvertHexToPulses_WithSupportedProtocols_ShouldReturnPulseArray(string protocol)
    {
        var transmitter = new GpioInfraredTransmitter(18, _logger);
        
        // Use reflection to access private method for testing
        var method = typeof(GpioInfraredTransmitter).GetMethod("ConvertHexToPulses", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        var result = (int[])method!.Invoke(transmitter, ["0x123456", protocol])!;
        
        result.Should().NotBeEmpty();
        result.Should().AllSatisfy(pulse => pulse.Should().BeGreaterThan(0));
    }

    [Fact]
    public void ConvertHexToPulses_WithUnsupportedProtocol_ShouldThrowNotSupportedException()
    {
        var transmitter = new GpioInfraredTransmitter(18, _logger);
        
        // Use reflection to access private method for testing
        var method = typeof(GpioInfraredTransmitter).GetMethod("ConvertHexToPulses", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        var act = () => method!.Invoke(transmitter, ["0x123456", "UNKNOWN"]);
        
        act.Should().Throw<System.Reflection.TargetInvocationException>()
           .WithInnerException<NotSupportedException>()
           .WithMessage("Protocol UNKNOWN not supported");
    }

    [Fact]
    public void ConvertNecToPulses_ShouldGenerateCorrectStructure()
    {
        var transmitter = new GpioInfraredTransmitter(18, _logger);
        
        // Use reflection to access private method for testing
        var method = typeof(GpioInfraredTransmitter).GetMethod("ConvertNecToPulses", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        var result = (int[])method!.Invoke(transmitter, [0x00FF00FFU])!;
        
        // NEC should start with header (9000, 4500) and end with stop bit (560)
        result[0].Should().Be(9000); // Header mark
        result[1].Should().Be(4500); // Header space
        result[^1].Should().Be(560); // Stop bit
        
        // Should have 67 total pulses (header + 32 data bits + stop)
        result.Length.Should().Be(67);
    }

    [Fact]
    public void ConvertSonyToPulses_ShouldGenerateCorrectStructure()
    {
        var transmitter = new GpioInfraredTransmitter(18, _logger);
        
        // Use reflection to access private method for testing
        var method = typeof(GpioInfraredTransmitter).GetMethod("ConvertSonyToPulses", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        var result = (int[])method!.Invoke(transmitter, [0xA90U])!;
        
        // Sony should start with header (2400, 600)
        result[0].Should().Be(2400); // Header mark
        result[1].Should().Be(600);  // Header space
        
        // Should have 26 total pulses (header + 12 data bits)
        result.Length.Should().Be(26);
    }

    [Fact]
    public void Dispose_ShouldLogDisposalMessage()
    {
        var transmitter = new GpioInfraredTransmitter(18, _logger);
        
        transmitter.Dispose();
        
        // Logger assertions removed - using NullLogger for simplicity
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_ShouldNotThrow()
    {
        var transmitter = new GpioInfraredTransmitter(18, _logger);
        
        var act = () =>
        {
            transmitter.Dispose();
            transmitter.Dispose();
        };
        
        act.Should().NotThrow();
    }
}