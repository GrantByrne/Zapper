using System.ComponentModel.DataAnnotations;

namespace Zapper.Core.Models;

public class IrCodeSet
{
    public int Id { get; set; }

    [Required]
    public string Brand { get; set; } = "";

    [Required]
    public string Model { get; set; } = "";

    [Required]
    public DeviceType DeviceType { get; set; }

    public string? Description { get; set; }

    public bool IsVerified { get; set; }

    public int DownloadCount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<IrCode> Codes { get; set; } = new();
}