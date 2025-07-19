namespace Zapper.Core.Models;

public class SystemInfo
{
    public string Platform { get; set; } = string.Empty;
    public bool IsRaspberryPi { get; set; }
    public bool HasGpioSupport { get; set; }
    public List<string> UserGroups { get; set; } = new();
    public List<string> GpioWarnings { get; set; } = new();
}