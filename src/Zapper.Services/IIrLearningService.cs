using Zapper.Core.Models;

namespace Zapper.Services;

public interface IIrLearningService
{
    Task<IrCode?> LearnCommandAsync(string commandName, TimeSpan timeout, CancellationToken cancellationToken = default);
    Task<int[]?> LearnRawCommandAsync(TimeSpan timeout, CancellationToken cancellationToken = default);
    bool IsReceiverAvailable { get; }
}