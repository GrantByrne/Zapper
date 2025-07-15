using Zapper.Core.Models;

namespace Zapper.Services;

public class IrCodeSetImport
{
    public string Brand { get; set; } = "";
    public string Model { get; set; } = "";
    public DeviceType DeviceType { get; set; }
    public string? Description { get; set; }
    public List<IrCodeImport> Codes { get; set; } = [];
}