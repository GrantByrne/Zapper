using Zapper.Core.Models;

namespace Zapper.Device.Infrared;

public interface IInfraredReceiver
{
    Task<IrCode?> ReceiveAsync(TimeSpan timeout, CancellationToken cancellationToken = default);
    Task<int[]?> ReceiveRawAsync(TimeSpan timeout, CancellationToken cancellationToken = default);
    bool IsAvailable { get; }
    void Initialize();
    void Dispose();
}