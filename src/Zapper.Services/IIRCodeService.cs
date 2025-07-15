using Zapper.Core.Models;

namespace Zapper.Services;

public interface IIrCodeService
{
    Task<IEnumerable<IrCodeSet>> GetCodeSetsAsync();
    Task<IEnumerable<IrCodeSet>> SearchCodeSetsAsync(string? brand = null, string? model = null, DeviceType? deviceType = null);
    Task<IrCodeSet?> GetCodeSetAsync(int id);
    Task<IrCodeSet?> GetCodeSetAsync(string brand, string model, DeviceType deviceType);
    Task<IEnumerable<IrCode>> GetCodesAsync(int codeSetId);
    Task<IrCode?> GetCodeAsync(int codeSetId, string commandName);
    Task<IrCodeSet> CreateCodeSetAsync(IrCodeSet codeSet);
    Task<IrCode> AddCodeAsync(int codeSetId, IrCode code);
    Task UpdateCodeSetAsync(IrCodeSet codeSet);
    Task DeleteCodeSetAsync(int id);
    Task<bool> ImportCodeSetAsync(string filePath);
    Task<string> ExportCodeSetAsync(int id);
    Task SeedDefaultCodesAsync();
    Task<IEnumerable<string>> GetExternalManufacturersAsync();
    Task<IEnumerable<(string Manufacturer, string DeviceType, string Device, string Subdevice)>> SearchExternalDevicesAsync(string? manufacturer = null, string? deviceType = null);
    Task<IrCodeSet?> GetExternalCodeSetAsync(string manufacturer, string deviceType, string device, string subdevice);
    Task<IrCodeSet> ImportExternalCodeSetAsync(string manufacturer, string deviceType, string device, string subdevice);
}