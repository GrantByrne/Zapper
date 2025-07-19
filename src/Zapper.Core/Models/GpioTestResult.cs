namespace Zapper.Core.Models;

public class GpioTestResult
{
    public bool IsAvailable { get; set; }
    public bool CanAccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public int Pin { get; set; }
    public string PinMode { get; set; } = string.Empty;
    public string? ErrorDetails { get; set; }
}