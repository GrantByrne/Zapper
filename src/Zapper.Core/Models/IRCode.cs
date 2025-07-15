using System.ComponentModel.DataAnnotations;

namespace Zapper.Core.Models;

public class IrCode
{
    public int Id { get; set; }

    [Required]
    public string Brand { get; set; } = "";

    [Required]
    public string Model { get; set; } = "";

    [Required]
    public DeviceType DeviceType { get; set; }

    [Required]
    public string CommandName { get; set; } = "";

    [Required]
    public string Protocol { get; set; } = "";

    [Required]
    public string HexCode { get; set; } = "";

    public int Frequency { get; set; } = 38000;

    public string? RawData { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}