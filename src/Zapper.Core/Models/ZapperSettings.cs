namespace Zapper.Core.Models;

public class ZapperSettings
{
    public HardwareSettings Hardware { get; set; } = new();
    public AdvancedSettings Advanced { get; set; } = new();
}