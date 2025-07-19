namespace Zapper.Core.Models;

public class IrHardwareSettings
{
    public int TransmitterGpioPin { get; set; } = 18;
    public int ReceiverGpioPin { get; set; } = 19;
    public int CarrierFrequency { get; set; } = 38000;
    public double DutyCycle { get; set; } = 0.33;
}