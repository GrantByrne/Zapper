namespace Zapper.Core.Models;

public class IrHardwareTestResult
{
    public bool IsAvailable { get; set; }
    public bool TestPassed { get; set; }
    public string Message { get; set; } = string.Empty;
    public int GpioPin { get; set; }
    public string? ErrorDetails { get; set; }
}