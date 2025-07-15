namespace Zapper.Core.Models;

public class ExternalIrCodeEntry
{
    public required string Function { get; set; }
    public required string Protocol { get; set; }
    public required string Device { get; set; }
    public required string Subdevice { get; set; }
    public required string FunctionCode { get; set; }
}