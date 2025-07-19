namespace Zapper.Core.Models;

public class HardwareSettings
{
    public bool EnableGpio { get; set; } = true;
    public bool EnableUsbRemotes { get; set; } = true;
    public IrHardwareSettings Infrared { get; set; } = new();
}