using Zapper.Core.Models;
using Microsoft.Extensions.Logging;

namespace Zapper.Integrations;

public class MockInfraredTransmitter : IInfraredTransmitter
{
    private readonly ILogger<MockInfraredTransmitter> _logger;
    private bool _isInitialized;

    public MockInfraredTransmitter(ILogger<MockInfraredTransmitter> logger)
    {
        _logger = logger;
    }

    public bool IsAvailable => _isInitialized;

    public void Initialize()
    {
        _isInitialized = true;
        _logger.LogInformation("Mock IR transmitter initialized");
    }

    public async Task TransmitAsync(string irCode, int repeatCount = 1, CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
            throw new InvalidOperationException("Mock IR transmitter not initialized");

        _logger.LogInformation("Mock transmitting IR code: {IrCode} (repeat {RepeatCount}x)", irCode, repeatCount);
        
        // Simulate transmission time
        await Task.Delay(100 * repeatCount, cancellationToken);
    }

    public async Task TransmitAsync(IRCode irCode, int repeatCount = 1, CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
            throw new InvalidOperationException("Mock IR transmitter not initialized");

        _logger.LogInformation("Mock transmitting IR code: {Brand} {Model} {Command} - {HexCode} (repeat {RepeatCount}x)", 
                              irCode.Brand, irCode.Model, irCode.CommandName, irCode.HexCode, repeatCount);
        
        // Simulate transmission time
        await Task.Delay(100 * repeatCount, cancellationToken);
    }

    public async Task TransmitRawAsync(int[] pulses, int carrierFrequency = 38000, CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
            throw new InvalidOperationException("Mock IR transmitter not initialized");

        _logger.LogInformation("Mock transmitting raw IR signal: {PulseCount} pulses at {Frequency}Hz", 
                              pulses.Length, carrierFrequency);
        
        // Simulate transmission time based on pulse count
        var totalMicros = pulses.Sum();
        await Task.Delay(Math.Max(1, totalMicros / 10000), cancellationToken);
    }

    public void Dispose()
    {
        _logger.LogInformation("Mock IR transmitter disposed");
    }
}