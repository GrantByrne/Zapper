using Zapper.Core.Models;

namespace Zapper.Services;

public class IrCodeSetImport
{
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public DeviceType DeviceType { get; set; }
    public string? Description { get; set; }
    public List<IrCodeImport> Codes { get; set; } = [];
}