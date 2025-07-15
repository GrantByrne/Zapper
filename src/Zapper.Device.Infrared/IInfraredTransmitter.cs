using Zapper.Core.Models;

namespace Zapper.Device.Infrared;

public interface IInfraredTransmitter
{
    Task Transmit(string irCode, int repeatCount = 1, CancellationToken cancellationToken = default);
    Task Transmit(IrCode irCode, int repeatCount = 1, CancellationToken cancellationToken = default);
    Task TransmitRaw(int[] pulses, int carrierFrequency = 38000, CancellationToken cancellationToken = default);
    bool IsAvailable { get; }
    void Initialize();
    void Dispose();
}