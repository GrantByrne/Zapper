using Zapper.Core.Models;

namespace Zapper.Hardware;

public interface IInfraredTransmitter
{
    Task TransmitAsync(string irCode, int repeatCount = 1, CancellationToken cancellationToken = default);
    Task TransmitAsync(IRCode irCode, int repeatCount = 1, CancellationToken cancellationToken = default);
    Task TransmitRawAsync(int[] pulses, int carrierFrequency = 38000, CancellationToken cancellationToken = default);
    bool IsAvailable { get; }
    void Initialize();
    void Dispose();
}