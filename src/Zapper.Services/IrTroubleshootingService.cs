using System.Device.Gpio;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Zapper.Core.Models;
using Zapper.Device.Infrared;

namespace Zapper.Services;

public class IrTroubleshootingService(
    IInfraredTransmitter infraredTransmitter,
    IInfraredReceiver infraredReceiver,
    ISettingsService settingsService,
    ILogger<IrTroubleshootingService> logger) : IIrTroubleshootingService
{
    public async Task<IrHardwareTestResult> TestIrTransmitterAsync()
    {
        var settings = await settingsService.GetSettingsAsync();
        var pin = settings.Hardware.Infrared.TransmitterGpioPin;

        logger.LogInformation("Testing IR transmitter on GPIO pin {Pin}", pin);

        try
        {
            if (!infraredTransmitter.IsAvailable)
            {
                infraredTransmitter.Initialize();
            }

            var testPulses = new int[] { 9000, 4500, 560, 1690, 560, 560, 560 };
            await infraredTransmitter.TransmitRaw(testPulses);

            return new IrHardwareTestResult
            {
                IsAvailable = true,
                TestPassed = true,
                Message = "IR transmitter test passed - test signal sent successfully",
                GpioPin = pin
            };
        }
        catch (UnauthorizedAccessException ex)
        {
            return new IrHardwareTestResult
            {
                IsAvailable = false,
                TestPassed = false,
                Message = "GPIO access denied - application may need to run with elevated privileges",
                GpioPin = pin,
                ErrorDetails = ex.Message
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "IR transmitter test failed");
            return new IrHardwareTestResult
            {
                IsAvailable = false,
                TestPassed = false,
                Message = $"IR transmitter test failed: {ex.Message}",
                GpioPin = pin,
                ErrorDetails = ex.ToString()
            };
        }
    }

    public async Task<IrHardwareTestResult> TestIrReceiverAsync(TimeSpan timeout)
    {
        var settings = await settingsService.GetSettingsAsync();
        var pin = settings.Hardware.Infrared.ReceiverGpioPin;

        logger.LogInformation("Testing IR receiver on GPIO pin {Pin} with timeout {Timeout}", pin, timeout);

        try
        {
            if (!infraredReceiver.IsAvailable)
            {
                infraredReceiver.Initialize();
            }

            var pulses = await infraredReceiver.ReceiveRawAsync(timeout);

            if (pulses != null && pulses.Length > 0)
            {
                return new IrHardwareTestResult
                {
                    IsAvailable = true,
                    TestPassed = true,
                    Message = $"IR receiver test passed - received signal with {pulses.Length} pulses",
                    GpioPin = pin
                };
            }
            else
            {
                return new IrHardwareTestResult
                {
                    IsAvailable = true,
                    TestPassed = false,
                    Message = $"IR receiver is available but no signal received within {timeout.TotalSeconds} seconds",
                    GpioPin = pin
                };
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            return new IrHardwareTestResult
            {
                IsAvailable = false,
                TestPassed = false,
                Message = "GPIO access denied - application may need to run with elevated privileges",
                GpioPin = pin,
                ErrorDetails = ex.Message
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "IR receiver test failed");
            return new IrHardwareTestResult
            {
                IsAvailable = false,
                TestPassed = false,
                Message = $"IR receiver test failed: {ex.Message}",
                GpioPin = pin,
                ErrorDetails = ex.ToString()
            };
        }
    }

    public async Task<GpioTestResult> TestGpioPinAsync(int pin, bool isOutput)
    {
        logger.LogInformation("Testing GPIO pin {Pin} as {Mode}", pin, isOutput ? "output" : "input");

        try
        {
            using var controller = new GpioController();
            var pinMode = isOutput ? PinMode.Output : PinMode.Input;

            controller.OpenPin(pin, pinMode);

            if (isOutput)
            {
                controller.Write(pin, PinValue.Low);
                await Task.Delay(100);
                controller.Write(pin, PinValue.High);
                await Task.Delay(100);
                controller.Write(pin, PinValue.Low);
            }
            else
            {
                var value = controller.Read(pin);
                logger.LogDebug("GPIO pin {Pin} current value: {Value}", pin, value);
            }

            return new GpioTestResult
            {
                IsAvailable = true,
                CanAccess = true,
                Message = $"GPIO pin {pin} test passed",
                Pin = pin,
                PinMode = pinMode.ToString()
            };
        }
        catch (UnauthorizedAccessException ex)
        {
            return new GpioTestResult
            {
                IsAvailable = false,
                CanAccess = false,
                Message = "GPIO access denied - application may need elevated privileges",
                Pin = pin,
                PinMode = isOutput ? "Output" : "Input",
                ErrorDetails = ex.Message
            };
        }
        catch (ArgumentException ex) when (ex.Message.Contains("pin") || ex.Message.Contains("Pin"))
        {
            return new GpioTestResult
            {
                IsAvailable = false,
                CanAccess = false,
                Message = $"GPIO pin {pin} is not available or already in use",
                Pin = pin,
                PinMode = isOutput ? "Output" : "Input",
                ErrorDetails = ex.Message
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GPIO pin {Pin} test failed", pin);
            return new GpioTestResult
            {
                IsAvailable = false,
                CanAccess = false,
                Message = $"GPIO pin {pin} test failed: {ex.Message}",
                Pin = pin,
                PinMode = isOutput ? "Output" : "Input",
                ErrorDetails = ex.ToString()
            };
        }
    }

    public async Task<SystemInfoResult> GetSystemInfoAsync()
    {
        await Task.CompletedTask;

        var result = new SystemInfoResult
        {
            Platform = RuntimeInformation.OSDescription,
            OsDescription = Environment.OSVersion.ToString()
        };

        result.IsRaspberryPi = IsRaspberryPi();
        result.HasGpioSupport = HasGpioSupport();
        result.IsRunningAsRoot = IsRunningAsRoot();

        if (!result.IsRaspberryPi)
        {
            result.GpioWarnings.Add("This application is designed to run on Raspberry Pi for GPIO functionality");
        }

        if (!result.HasGpioSupport)
        {
            result.GpioWarnings.Add("GPIO support not detected on this system");
        }

        if (!result.IsRunningAsRoot && result.IsRaspberryPi)
        {
            result.GpioWarnings.Add("Application is not running with elevated privileges - GPIO access may be limited");
        }

        return result;
    }

    private static bool IsRaspberryPi()
    {
        try
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return false;

            if (File.Exists("/proc/cpuinfo"))
            {
                var cpuInfo = File.ReadAllText("/proc/cpuinfo").ToLowerInvariant();
                return cpuInfo.Contains("raspberry pi") || cpuInfo.Contains("bcm2");
            }

            if (File.Exists("/proc/device-tree/model"))
            {
                var model = File.ReadAllText("/proc/device-tree/model").ToLowerInvariant();
                return model.Contains("raspberry pi");
            }
        }
        catch (Exception)
        {
            // Ignore errors when checking files
        }

        return false;
    }

    private static bool HasGpioSupport()
    {
        try
        {
            using var controller = new GpioController();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static bool IsRunningAsRoot()
    {
        try
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return false;

            return Environment.UserName.Equals("root", StringComparison.OrdinalIgnoreCase) ||
                   Environment.GetEnvironmentVariable("USER")?.Equals("root", StringComparison.OrdinalIgnoreCase) == true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}