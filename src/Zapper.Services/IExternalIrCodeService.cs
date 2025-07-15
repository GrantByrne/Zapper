using Zapper.Core.Models;

namespace Zapper.Services;

public interface IExternalIrCodeService
{
    Task<IEnumerable<string>> GetAvailableManufacturersAsync();
    Task<IEnumerable<(string Manufacturer, string DeviceType, string Device, string Subdevice)>> SearchDevicesAsync(string? manufacturer = null, string? deviceType = null);
    Task<IrCodeSet?> GetCodeSetAsync(string manufacturer, string deviceType, string device, string subdevice);
    Task<bool> IsAvailableAsync();
    Task InvalidateCacheAsync();
}