namespace Zapper.Services;

public class IrCodeImport
{
    public string CommandName { get; set; } = string.Empty;
    public string Protocol { get; set; } = string.Empty;
    public string HexCode { get; set; } = string.Empty;
    public int Frequency { get; set; } = 38000;
    public string? RawData { get; set; }
    public string? Notes { get; set; }
}