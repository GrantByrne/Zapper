using Zapper.Core.Models;

namespace Zapper.Services;

public interface IIrTroubleshootingService
{
    Task<IrHardwareTestResult> TestIrTransmitterAsync();
    Task<IrHardwareTestResult> TestIrReceiverAsync(TimeSpan timeout);
    Task<GpioTestResult> TestGpioPinAsync(int pin, bool isOutput);
    Task<SystemInfoResult> GetSystemInfoAsync();
}