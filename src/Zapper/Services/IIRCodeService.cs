using Zapper.Models;

namespace Zapper.Services;

public interface IIRCodeService
{
    Task<IEnumerable<IRCodeSet>> GetCodeSetsAsync();
    Task<IEnumerable<IRCodeSet>> SearchCodeSetsAsync(string? brand = null, string? model = null, DeviceType? deviceType = null);
    Task<IRCodeSet?> GetCodeSetAsync(int id);
    Task<IRCodeSet?> GetCodeSetAsync(string brand, string model, DeviceType deviceType);
    Task<IEnumerable<IRCode>> GetCodesAsync(int codeSetId);
    Task<IRCode?> GetCodeAsync(int codeSetId, string commandName);
    Task<IRCodeSet> CreateCodeSetAsync(IRCodeSet codeSet);
    Task<IRCode> AddCodeAsync(int codeSetId, IRCode code);
    Task UpdateCodeSetAsync(IRCodeSet codeSet);
    Task DeleteCodeSetAsync(int id);
    Task<bool> ImportCodeSetAsync(string filePath);
    Task<string> ExportCodeSetAsync(int id);
    Task SeedDefaultCodesAsync();
}