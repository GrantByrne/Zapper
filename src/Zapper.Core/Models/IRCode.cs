using System.ComponentModel.DataAnnotations;

namespace Zapper.Core.Models;

public class IrCode
{
    public int Id { get; set; }

    [Required]
    public string Brand { get; set; } = string.Empty;

    [Required]
    public string Model { get; set; } = string.Empty;

    [Required]
    public DeviceType DeviceType { get; set; }

    [Required]
    public string CommandName { get; set; } = string.Empty;

    [Required]
    public string Protocol { get; set; } = string.Empty;

    [Required]
    public string HexCode { get; set; } = string.Empty;

    public int Frequency { get; set; } = 38000;

    public string? RawData { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class IrCodeSet
{
    public int Id { get; set; }

    [Required]
    public string Brand { get; set; } = string.Empty;

    [Required]
    public string Model { get; set; } = string.Empty;

    [Required]
    public DeviceType DeviceType { get; set; }

    public string? Description { get; set; }

    public bool IsVerified { get; set; }

    public int DownloadCount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<IrCode> Codes { get; set; } = new();
}