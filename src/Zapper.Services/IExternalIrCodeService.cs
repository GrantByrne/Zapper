using Zapper.Core.Models;

namespace Zapper.Services;

public interface IExternalIrCodeService
{
    Task<IEnumerable<string>> GetAvailableManufacturers();
    Task<IEnumerable<(string Manufacturer, string DeviceType, string Device, string Subdevice)>> SearchDevices(string? manufacturer = null, string? deviceType = null);
    Task<IrCodeSet?> GetCodeSet(string manufacturer, string deviceType, string device, string subdevice);
    Task<bool> IsAvailable();
    Task InvalidateCache();
}