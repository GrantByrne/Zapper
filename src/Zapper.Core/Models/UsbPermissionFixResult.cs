namespace Zapper.Core.Models;

public class UsbPermissionFixResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Steps { get; set; } = new();
    public bool RequiresRestart { get; set; }
}