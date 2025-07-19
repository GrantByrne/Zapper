using Zapper.Core.Models;
using Zapper.Device.Infrared;
using Microsoft.Extensions.Logging;

namespace Zapper.Services;

public class IrLearningService(IInfraredReceiver infraredReceiver, ILogger<IrLearningService> logger) : IIrLearningService
{
    public bool IsReceiverAvailable => infraredReceiver.IsAvailable;

    public async Task<IrCode?> LearnCommandAsync(string commandName, TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        if (!infraredReceiver.IsAvailable)
        {
            logger.LogWarning("IR receiver not available for learning");
            return null;
        }

        logger.LogInformation("Starting IR learning for command: {CommandName}", commandName);

        try
        {
            var irCode = await infraredReceiver.ReceiveAsync(timeout, cancellationToken);
            if (irCode != null)
            {
                irCode.CommandName = commandName;
                logger.LogInformation("Successfully learned IR command: {CommandName} with protocol {Protocol}",
                    commandName, irCode.Protocol);
            }
            else
            {
                logger.LogWarning("Failed to learn IR command: {CommandName} - no signal received", commandName);
            }

            return irCode;
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("IR learning cancelled for command: {CommandName}", commandName);
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error learning IR command: {CommandName}", commandName);
            throw;
        }
    }

    public async Task<int[]?> LearnRawCommandAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        if (!infraredReceiver.IsAvailable)
        {
            logger.LogWarning("IR receiver not available for raw learning");
            return null;
        }

        logger.LogInformation("Starting raw IR learning");

        try
        {
            var pulses = await infraredReceiver.ReceiveRawAsync(timeout, cancellationToken);
            if (pulses != null)
            {
                logger.LogInformation("Successfully learned raw IR signal with {PulseCount} pulses", pulses.Length);
            }
            else
            {
                logger.LogWarning("Failed to learn raw IR signal - no signal received");
            }

            return pulses;
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Raw IR learning cancelled");
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error learning raw IR signal");
            throw;
        }
    }
}