using Zapper.Core.Models;

namespace Zapper.Services;

public interface ISystemDiagnosticsService
{
    Task<UsbPermissionStatus> CheckUsbPermissionsAsync();
    Task<UsbPermissionFixResult> FixUsbPermissionsAsync(string password);
    Task<SystemInfo> GetSystemInfoAsync();
}