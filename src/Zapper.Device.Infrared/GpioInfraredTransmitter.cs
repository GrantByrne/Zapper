using System.Device.Gpio;
using System.Device.Pwm;
using Zapper.Core.Models;
using Microsoft.Extensions.Logging;

namespace Zapper.Device.Infrared;

public class GpioInfraredTransmitter(int gpioPin, ILogger<GpioInfraredTransmitter> logger)
    : IInfraredTransmitter, IDisposable
{
    private GpioController? _controller;
    private PwmChannel? _pwmChannel;
    private bool _isInitialized;
    private bool _disposed;

    public bool IsAvailable => _isInitialized && !_disposed;

    public void Initialize()
    {
        try
        {
            _controller = new GpioController();
            _controller.OpenPin(gpioPin, PinMode.Output);

            try
            {
                _pwmChannel = PwmChannel.Create(0, 0, 38000);
                _pwmChannel.DutyCycle = 0.33;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "PWM not available, falling back to GPIO timing");
            }

            _isInitialized = true;
            logger.LogInformation("IR transmitter initialized on GPIO pin {Pin}", gpioPin);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to initialize IR transmitter on GPIO pin {Pin}", gpioPin);
            throw;
        }
    }

    public async Task Transmit(string irCode, int repeatCount = 1, CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
            throw new InvalidOperationException("IR transmitter not initialized");

        var pulses = ParseIrCode(irCode);

        for (int i = 0; i < repeatCount; i++)
        {
            await TransmitRaw(pulses, cancellationToken: cancellationToken);
            if (i < repeatCount - 1)
            {
                await Task.Delay(100, cancellationToken);
            }
        }
    }

    public async Task Transmit(IrCode irCode, int repeatCount = 1, CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
            throw new InvalidOperationException("IR transmitter not initialized");

        logger.LogInformation("Transmitting IR code for {Brand} {Model} command {Command}",
                              irCode.Brand, irCode.Model, irCode.CommandName);

        int[] pulses;

        if (!string.IsNullOrEmpty(irCode.RawData))
        {
            pulses = ParseIrCode(irCode.RawData);
        }
        else
        {
            pulses = ConvertHexToPulses(irCode.HexCode, irCode.Protocol);
        }

        for (int i = 0; i < repeatCount; i++)
        {
            await TransmitRaw(pulses, irCode.Frequency, cancellationToken);
            if (i < repeatCount - 1)
            {
                await Task.Delay(100, cancellationToken);
            }
        }
    }

    public async Task TransmitRaw(int[] pulses, int carrierFrequency = 38000, CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
            throw new InvalidOperationException("IR transmitter not initialized");

        logger.LogDebug("Transmitting IR signal with {PulseCount} pulses", pulses.Length);

        try
        {
            for (int i = 0; i < pulses.Length; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                bool isHigh = i % 2 == 0;
                int durationMicros = pulses[i];

                if (isHigh)
                {
                    if (_pwmChannel != null)
                    {
                        _pwmChannel.Start();
                        await Task.Delay(TimeSpan.FromMicroseconds(durationMicros), cancellationToken);
                        _pwmChannel.Stop();
                    }
                    else
                    {
                        await TransmitCarrierAsync(durationMicros, carrierFrequency, cancellationToken);
                    }
                }
                else
                {
                    _controller!.Write(gpioPin, PinValue.Low);
                    await Task.Delay(TimeSpan.FromMicroseconds(durationMicros), cancellationToken);
                }
            }
        }
        finally
        {
            _controller!.Write(gpioPin, PinValue.Low);
        }
    }

    private async Task TransmitCarrierAsync(int durationMicros, int frequency, CancellationToken cancellationToken)
    {
        var halfPeriodMicros = 1_000_000 / (frequency * 2);
        var endTime = DateTime.UtcNow.AddMicroseconds(durationMicros);

        while (DateTime.UtcNow < endTime && !cancellationToken.IsCancellationRequested)
        {
            _controller!.Write(gpioPin, PinValue.High);
            await Task.Delay(TimeSpan.FromMicroseconds(halfPeriodMicros), cancellationToken);
            _controller!.Write(gpioPin, PinValue.Low);
            await Task.Delay(TimeSpan.FromMicroseconds(halfPeriodMicros), cancellationToken);
        }
    }

    private int[] ParseIrCode(string irCode)
    {
        try
        {
            return irCode.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                        .Select(int.Parse)
                        .ToArray();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to parse IR code: {IrCode}", irCode);
            throw new ArgumentException("Invalid IR code format", nameof(irCode));
        }
    }

    private int[] ConvertHexToPulses(string hexCode, string protocol)
    {
        try
        {
            var code = Convert.ToUInt32(hexCode.Replace("0x", ""), 16);

            return protocol.ToUpper() switch
            {
                "NEC" => ConvertNecToPulses(code),
                "SONY" => ConvertSonyToPulses(code),
                "RC5" => ConvertRc5ToPulses(code),
                "RC6" => ConvertRc6ToPulses(code),
                _ => throw new NotSupportedException($"Protocol {protocol} not supported")
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to convert hex code to pulses: {HexCode} ({Protocol})", hexCode, protocol);
            throw;
        }
    }

    private int[] ConvertNecToPulses(uint code)
    {
        var pulses = new List<int>();

        pulses.Add(9000);
        pulses.Add(4500);
        for (int i = 31; i >= 0; i--)
        {
            pulses.Add(560);
            if ((code >> i & 1) == 1)
            {
                pulses.Add(1690);
            }
            else
            {
                pulses.Add(560);
            }
        }

        pulses.Add(560);

        return pulses.ToArray();
    }

    private int[] ConvertSonyToPulses(uint code)
    {
        var pulses = new List<int>();

        pulses.Add(2400);
        pulses.Add(600);
        for (int i = 11; i >= 0; i--)
        {
            if ((code >> i & 1) == 1)
            {
                pulses.Add(1200);
            }
            else
            {
                pulses.Add(600);
            }
            pulses.Add(600);
        }

        return pulses.ToArray();
    }

    private int[] ConvertRc5ToPulses(uint code)
    {
        var pulses = new List<int> { 889, 889 };

        for (int i = 12; i >= 0; i--)
        {
            if ((code >> i & 1) == 1)
            {
                pulses.Add(889);
                pulses.Add(889);
            }
            else
            {
                pulses.Add(889);
                pulses.Add(889);
            }
        }

        return pulses.ToArray();
    }

    private int[] ConvertRc6ToPulses(uint code)
    {
        var pulses = new List<int> { 2666, 889 };

        for (int i = 15; i >= 0; i--)
        {
            if ((code >> i & 1) == 1)
            {
                pulses.Add(444);
                pulses.Add(444);
            }
            else
            {
                pulses.Add(444);
                pulses.Add(444);
            }
        }

        return pulses.ToArray();
    }

    public void Dispose()
    {
        if (_disposed) return;

        _pwmChannel?.Dispose();
        _controller?.Dispose();
        _disposed = true;

        logger.LogInformation("IR transmitter disposed");
    }
}