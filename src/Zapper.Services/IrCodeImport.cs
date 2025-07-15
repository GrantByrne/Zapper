namespace Zapper.Services;

public class IrCodeImport
{
    public string CommandName { get; set; } = "";
    public string Protocol { get; set; } = "";
    public string HexCode { get; set; } = "";
    public int Frequency { get; set; } = 38000;
    public string? RawData { get; set; }
    public string? Notes { get; set; }
}