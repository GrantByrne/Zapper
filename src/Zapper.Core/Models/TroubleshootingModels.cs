namespace Zapper.Core.Models;

public class IrHardwareTestResult
{
    public bool IsAvailable { get; set; }
    public bool TestPassed { get; set; }
    public string Message { get; set; } = string.Empty;
    public int GpioPin { get; set; }
    public string? ErrorDetails { get; set; }
}

public class GpioTestResult
{
    public bool IsAvailable { get; set; }
    public bool CanAccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public int Pin { get; set; }
    public string PinMode { get; set; } = string.Empty;
    public string? ErrorDetails { get; set; }
}

public class SystemInfoResult
{
    public bool IsRaspberryPi { get; set; }
    public bool HasGpioSupport { get; set; }
    public string Platform { get; set; } = string.Empty;
    public string OsDescription { get; set; } = string.Empty;
    public bool IsRunningAsRoot { get; set; }
    public List<string> GpioWarnings { get; set; } = new();
}