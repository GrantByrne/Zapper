using System.Device.Gpio;
using System.Device.Pwm;

namespace ZapperHub.Hardware;

public class GpioInfraredTransmitter : IInfraredTransmitter, IDisposable
{
    private readonly int _gpioPin;
    private readonly ILogger<GpioInfraredTransmitter> _logger;
    private GpioController? _controller;
    private PwmChannel? _pwmChannel;
    private bool _isInitialized;
    private bool _disposed;

    public GpioInfraredTransmitter(int gpioPin, ILogger<GpioInfraredTransmitter> logger)
    {
        _gpioPin = gpioPin;
        _logger = logger;
    }

    public bool IsAvailable => _isInitialized && !_disposed;

    public void Initialize()
    {
        try
        {
            _controller = new GpioController();
            _controller.OpenPin(_gpioPin, PinMode.Output);
            
            // Try to initialize PWM for more accurate timing
            try
            {
                _pwmChannel = PwmChannel.Create(0, 0, 38000); // 38kHz carrier frequency
                _pwmChannel.DutyCycle = 0.33; // 33% duty cycle
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "PWM not available, falling back to GPIO timing");
            }
            
            _isInitialized = true;
            _logger.LogInformation("IR transmitter initialized on GPIO pin {Pin}", _gpioPin);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize IR transmitter on GPIO pin {Pin}", _gpioPin);
            throw;
        }
    }

    public async Task TransmitAsync(string irCode, int repeatCount = 1, CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
            throw new InvalidOperationException("IR transmitter not initialized");

        // Parse IR code (assume it's in Pronto format or similar)
        var pulses = ParseIrCode(irCode);
        
        for (int i = 0; i < repeatCount; i++)
        {
            await TransmitRawAsync(pulses, cancellationToken: cancellationToken);
            if (i < repeatCount - 1)
            {
                await Task.Delay(100, cancellationToken); // Gap between repeats
            }
        }
    }

    public async Task TransmitRawAsync(int[] pulses, int carrierFrequency = 38000, CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
            throw new InvalidOperationException("IR transmitter not initialized");

        _logger.LogDebug("Transmitting IR signal with {PulseCount} pulses", pulses.Length);

        try
        {
            for (int i = 0; i < pulses.Length; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                bool isHigh = i % 2 == 0; // Odd indices are marks (high), even are spaces (low)
                int durationMicros = pulses[i];
                
                if (isHigh)
                {
                    // Transmit carrier frequency
                    if (_pwmChannel != null)
                    {
                        _pwmChannel.Start();
                        await Task.Delay(TimeSpan.FromMicroseconds(durationMicros), cancellationToken);
                        _pwmChannel.Stop();
                    }
                    else
                    {
                        // Fallback: bit-bang the carrier frequency
                        await TransmitCarrierAsync(durationMicros, carrierFrequency, cancellationToken);
                    }
                }
                else
                {
                    // Space (no signal)
                    _controller!.Write(_gpioPin, PinValue.Low);
                    await Task.Delay(TimeSpan.FromMicroseconds(durationMicros), cancellationToken);
                }
            }
        }
        finally
        {
            // Ensure pin is low after transmission
            _controller!.Write(_gpioPin, PinValue.Low);
        }
    }

    private async Task TransmitCarrierAsync(int durationMicros, int frequency, CancellationToken cancellationToken)
    {
        var halfPeriodMicros = 1_000_000 / (frequency * 2);
        var endTime = DateTime.UtcNow.AddMicroseconds(durationMicros);
        
        while (DateTime.UtcNow < endTime && !cancellationToken.IsCancellationRequested)
        {
            _controller!.Write(_gpioPin, PinValue.High);
            await Task.Delay(TimeSpan.FromMicroseconds(halfPeriodMicros), cancellationToken);
            _controller!.Write(_gpioPin, PinValue.Low);
            await Task.Delay(TimeSpan.FromMicroseconds(halfPeriodMicros), cancellationToken);
        }
    }

    private int[] ParseIrCode(string irCode)
    {
        // Simple parser for space-separated microsecond values
        // In a real implementation, you'd support multiple formats (Pronto, Raw, etc.)
        try
        {
            return irCode.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                        .Select(int.Parse)
                        .ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse IR code: {IrCode}", irCode);
            throw new ArgumentException("Invalid IR code format", nameof(irCode));
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        
        _pwmChannel?.Dispose();
        _controller?.Dispose();
        _disposed = true;
        
        _logger.LogInformation("IR transmitter disposed");
    }
}