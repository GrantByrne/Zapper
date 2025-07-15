using System.Device.Gpio;
using Zapper.Core.Models;
using Microsoft.Extensions.Logging;

namespace Zapper.Device.Infrared;

public class GpioInfraredReceiver(int gpioPin, ILogger<GpioInfraredReceiver> logger)
    : IInfraredReceiver, IDisposable
{
    private GpioController? _controller;
    private bool _isInitialized;
    private bool _disposed;

    public bool IsAvailable => _isInitialized && !_disposed;

    public void Initialize()
    {
        try
        {
            _controller = new GpioController();
            _controller.OpenPin(gpioPin, PinMode.Input);

            _isInitialized = true;
            logger.LogInformation("IR receiver initialized on GPIO pin {Pin}", gpioPin);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to initialize IR receiver on GPIO pin {Pin}", gpioPin);
            throw;
        }
    }

    public async Task<IrCode?> ReceiveAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        var pulses = await ReceiveRawAsync(timeout, cancellationToken);
        if (pulses == null) return null;

        var irCode = DecodeIrSignal(pulses);
        if (irCode != null)
        {
            logger.LogInformation("Received IR signal - Protocol: {Protocol}, Command: {Command}",
                irCode.Protocol, irCode.HexCode);
        }

        return irCode;
    }

    public async Task<int[]?> ReceiveRawAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
            throw new InvalidOperationException("IR receiver not initialized");

        logger.LogDebug("Starting IR signal reception with timeout {Timeout}", timeout);

        var pulses = new List<int>();
        var startTime = DateTime.UtcNow;
        var lastValue = PinValue.Low;
        var lastTransitionTime = DateTime.UtcNow;
        var signalStarted = false;

        using var timeoutCts = new CancellationTokenSource(timeout);
        using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        try
        {
            while (!combinedCts.Token.IsCancellationRequested)
            {
                var currentValue = _controller!.Read(gpioPin);
                var currentTime = DateTime.UtcNow;

                if (currentValue != lastValue)
                {
                    if (signalStarted)
                    {
                        var duration = (int)(currentTime - lastTransitionTime).TotalMicroseconds;
                        if (duration > 50)
                        {
                            pulses.Add(duration);
                        }
                    }
                    else if (currentValue == PinValue.High)
                    {
                        signalStarted = true;
                        logger.LogDebug("IR signal detected, starting capture");
                    }

                    lastValue = currentValue;
                    lastTransitionTime = currentTime;
                }

                if (signalStarted && currentValue == PinValue.Low)
                {
                    var silenceDuration = (currentTime - lastTransitionTime).TotalMilliseconds;
                    if (silenceDuration > 50)
                    {
                        logger.LogDebug("IR signal capture complete with {PulseCount} pulses", pulses.Count);
                        return pulses.ToArray();
                    }
                }

                await Task.Delay(1, combinedCts.Token);
            }

            if (pulses.Count > 0)
            {
                logger.LogDebug("IR signal capture timeout with {PulseCount} pulses", pulses.Count);
                return pulses.ToArray();
            }

            logger.LogDebug("IR signal reception timeout - no signal detected");
            return null;
        }
        catch (OperationCanceledException)
        {
            logger.LogDebug("IR signal reception cancelled");
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during IR signal reception");
            throw;
        }
    }

    private IrCode? DecodeIrSignal(int[] pulses)
    {
        if (pulses.Length < 10) return null;

        var necCode = TryDecodeNec(pulses);
        if (necCode != null) return necCode;

        var sonyCode = TryDecodeSony(pulses);
        if (sonyCode != null) return sonyCode;

        var rc5Code = TryDecodeRc5(pulses);
        if (rc5Code != null) return rc5Code;

        var rc6Code = TryDecodeRc6(pulses);
        if (rc6Code != null) return rc6Code;

        return new IrCode
        {
            Brand = "Unknown",
            Model = "Unknown",
            CommandName = "Unknown",
            Protocol = "RAW",
            HexCode = "0x00000000",
            RawData = string.Join(" ", pulses),
            Frequency = 38000
        };
    }

    private IrCode? TryDecodeNec(int[] pulses)
    {
        if (pulses.Length < 67) return null;

        if (Math.Abs(pulses[0] - 9000) > 1000 || Math.Abs(pulses[1] - 4500) > 1000)
            return null;

        uint code = 0;
        for (int i = 0; i < 32; i++)
        {
            var markIndex = 2 + i * 2;
            var spaceIndex = markIndex + 1;

            if (markIndex >= pulses.Length || spaceIndex >= pulses.Length)
                return null;

            if (Math.Abs(pulses[markIndex] - 560) > 200)
                return null;

            if (Math.Abs(pulses[spaceIndex] - 1690) < 300)
            {
                code |= (uint)(1 << (31 - i));
            }
            else if (Math.Abs(pulses[spaceIndex] - 560) > 200)
            {
                return null;
            }
        }

        return new IrCode
        {
            Brand = "Unknown",
            Model = "Unknown",
            CommandName = "Unknown",
            Protocol = "NEC",
            HexCode = $"0x{code:X8}",
            RawData = string.Join(" ", pulses),
            Frequency = 38000
        };
    }

    private IrCode? TryDecodeSony(int[] pulses)
    {
        if (pulses.Length < 25) return null;

        if (Math.Abs(pulses[0] - 2400) > 400 || Math.Abs(pulses[1] - 600) > 200)
            return null;

        uint code = 0;
        for (int i = 0; i < 12; i++)
        {
            var markIndex = 2 + i * 2;
            var spaceIndex = markIndex + 1;

            if (markIndex >= pulses.Length || spaceIndex >= pulses.Length)
                return null;

            if (Math.Abs(pulses[spaceIndex] - 600) > 200)
                return null;

            if (Math.Abs(pulses[markIndex] - 1200) < 300)
            {
                code |= (uint)(1 << (11 - i));
            }
            else if (Math.Abs(pulses[markIndex] - 600) > 200)
            {
                return null;
            }
        }

        return new IrCode
        {
            Brand = "Unknown",
            Model = "Unknown",
            CommandName = "Unknown",
            Protocol = "SONY",
            HexCode = $"0x{code:X8}",
            RawData = string.Join(" ", pulses),
            Frequency = 40000
        };
    }

    private IrCode? TryDecodeRc5(int[] pulses)
    {
        if (pulses.Length < 26) return null;

        if (Math.Abs(pulses[0] - 889) > 200 || Math.Abs(pulses[1] - 889) > 200)
            return null;

        uint code = 0;
        for (int i = 0; i < 13; i++)
        {
            var markIndex = 2 + i * 2;
            var spaceIndex = markIndex + 1;

            if (markIndex >= pulses.Length || spaceIndex >= pulses.Length)
                return null;

            if (Math.Abs(pulses[markIndex] - 889) > 200 || Math.Abs(pulses[spaceIndex] - 889) > 200)
                return null;

            code |= (uint)(1 << (12 - i));
        }

        return new IrCode
        {
            Brand = "Unknown",
            Model = "Unknown",
            CommandName = "Unknown",
            Protocol = "RC5",
            HexCode = $"0x{code:X8}",
            RawData = string.Join(" ", pulses),
            Frequency = 36000
        };
    }

    private IrCode? TryDecodeRc6(int[] pulses)
    {
        if (pulses.Length < 34) return null;

        if (Math.Abs(pulses[0] - 2666) > 400 || Math.Abs(pulses[1] - 889) > 200)
            return null;

        uint code = 0;
        for (int i = 0; i < 16; i++)
        {
            var markIndex = 2 + i * 2;
            var spaceIndex = markIndex + 1;

            if (markIndex >= pulses.Length || spaceIndex >= pulses.Length)
                return null;

            if (Math.Abs(pulses[markIndex] - 444) > 150 || Math.Abs(pulses[spaceIndex] - 444) > 150)
                return null;

            code |= (uint)(1 << (15 - i));
        }

        return new IrCode
        {
            Brand = "Unknown",
            Model = "Unknown",
            CommandName = "Unknown",
            Protocol = "RC6",
            HexCode = $"0x{code:X8}",
            RawData = string.Join(" ", pulses),
            Frequency = 36000
        };
    }

    public void Dispose()
    {
        if (_disposed) return;

        _controller?.Dispose();
        _disposed = true;

        logger.LogInformation("IR receiver disposed");
    }
}