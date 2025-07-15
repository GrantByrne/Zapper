using Zapper.Core.Models;
using Microsoft.Extensions.Logging;

namespace Zapper.Device.Infrared;

public class MockInfraredTransmitter(ILogger<MockInfraredTransmitter> logger) : IInfraredTransmitter
{
    public bool IsAvailable { get; private set; }

    public void Initialize()
    {
        IsAvailable = true;
        logger.LogInformation("Mock IR transmitter initialized");
    }

    public async Task Transmit(string irCode, int repeatCount = 1, CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
            throw new InvalidOperationException("Mock IR transmitter not initialized");

        logger.LogInformation("Mock transmitting IR code: {IrCode} (repeat {RepeatCount}x)", irCode, repeatCount);

        await Task.Delay(100 * repeatCount, cancellationToken);
    }

    public async Task Transmit(IrCode irCode, int repeatCount = 1, CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
            throw new InvalidOperationException("Mock IR transmitter not initialized");

        logger.LogInformation("Mock transmitting IR code: {Brand} {Model} {Command} - {HexCode} (repeat {RepeatCount}x)",
                              irCode.Brand, irCode.Model, irCode.CommandName, irCode.HexCode, repeatCount);

        await Task.Delay(100 * repeatCount, cancellationToken);
    }

    public async Task TransmitRaw(int[] pulses, int carrierFrequency = 38000, CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
            throw new InvalidOperationException("Mock IR transmitter not initialized");

        logger.LogInformation("Mock transmitting raw IR signal: {PulseCount} pulses at {Frequency}Hz",
                              pulses.Length, carrierFrequency);

        var totalMicros = pulses.Sum();
        await Task.Delay(Math.Max(1, totalMicros / 10000), cancellationToken);
    }

    public void Dispose()
    {
        logger.LogInformation("Mock IR transmitter disposed");
    }
}