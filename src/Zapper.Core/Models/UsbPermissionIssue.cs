namespace Zapper.Core.Models;

public class UsbPermissionIssue
{
    public string DevicePath { get; set; } = string.Empty;
    public string VendorId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}