namespace Zapper.Core.Models;

public class ZapperSettings
{
    public HardwareSettings Hardware { get; set; } = new();
    public AdvancedSettings Advanced { get; set; } = new();
}

public class HardwareSettings
{
    public bool EnableGpio { get; set; } = true;
    public IrHardwareSettings Infrared { get; set; } = new();
}

public class IrHardwareSettings
{
    public int TransmitterGpioPin { get; set; } = 18;
    public int ReceiverGpioPin { get; set; } = 19;
    public int CarrierFrequency { get; set; } = 38000;
    public double DutyCycle { get; set; } = 0.33;
}

public class AdvancedSettings
{
    public bool EnableDebugLogging { get; set; } = false;
    public bool EnableTelemetry { get; set; } = true;
}