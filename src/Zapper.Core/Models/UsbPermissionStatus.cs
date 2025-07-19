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