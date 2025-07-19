namespace Zapper.Core.Models;

public class SystemInfoResult
{
    public bool IsRaspberryPi { get; set; }
    public bool HasGpioSupport { get; set; }
    public string Platform { get; set; } = string.Empty;
    public string OsDescription { get; set; } = string.Empty;
    public bool IsRunningAsRoot { get; set; }
    public List<string> GpioWarnings { get; set; } = new();
}