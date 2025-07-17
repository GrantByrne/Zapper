namespace Zapper.Core.Models;

public class UsbPermissionStatus
{
    public bool HasPermissionIssues { get; set; }
    public bool IsRunningAsRoot { get; set; }
    public bool UserInPlugdevGroup { get; set; }
    public bool UdevRulesExist { get; set; }
    public string Platform { get; set; } = string.Empty;
    public List<UsbPermissionIssue> Issues { get; set; } = new();
    public string? ErrorMessage { get; set; }
}

public class UsbPermissionIssue
{
    public string DevicePath { get; set; } = string.Empty;
    public string VendorId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}

public class UsbPermissionFixResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Steps { get; set; } = new();
    public bool RequiresRestart { get; set; }
}

public class SystemInfo
{
    public string Platform { get; set; } = string.Empty;
    public bool IsRaspberryPi { get; set; }
    public bool HasGpioSupport { get; set; }
    public List<string> UserGroups { get; set; } = new();
    public List<string> GpioWarnings { get; set; } = new();
}