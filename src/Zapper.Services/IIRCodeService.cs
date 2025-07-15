using Zapper.Core.Models;

namespace Zapper.Services;

public interface IIrCodeService
{
    Task<IEnumerable<IrCodeSet>> GetCodeSets();
    Task<IEnumerable<IrCodeSet>> SearchCodeSets(string? brand = null, string? model = null, DeviceType? deviceType = null);
    Task<IrCodeSet?> GetCodeSet(int id);
    Task<IrCodeSet?> GetCodeSet(string brand, string model, DeviceType deviceType);
    Task<IEnumerable<IrCode>> GetCodes(int codeSetId);
    Task<IrCode?> GetCode(int codeSetId, string commandName);
    Task<IrCodeSet> CreateCodeSet(IrCodeSet codeSet);
    Task<IrCode> AddCode(int codeSetId, IrCode code);
    Task UpdateCodeSet(IrCodeSet codeSet);
    Task DeleteCodeSet(int id);
    Task<bool> ImportCodeSet(string filePath);
    Task<string> ExportCodeSet(int id);
    Task SeedDefaultCodes();
    Task<IEnumerable<string>> GetExternalManufacturers();
    Task<IEnumerable<(string Manufacturer, string DeviceType, string Device, string Subdevice)>> SearchExternalDevices(string? manufacturer = null, string? deviceType = null);
    Task<IrCodeSet?> GetExternalCodeSet(string manufacturer, string deviceType, string device, string subdevice);
    Task<IrCodeSet> ImportExternalCodeSet(string manufacturer, string deviceType, string device, string subdevice);
}