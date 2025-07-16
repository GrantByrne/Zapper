using System.ComponentModel.DataAnnotations;

namespace Zapper.Core.Models;

public class UsbRemote
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = "";

    [Required]
    public int VendorId { get; set; }

    [Required]
    public int ProductId { get; set; }

    [MaxLength(50)]
    public string? SerialNumber { get; set; }

    [MaxLength(200)]
    public string? ProductName { get; set; }

    [MaxLength(200)]
    public string? Manufacturer { get; set; }

    [Required]
    public string DeviceId { get; set; } = "";

    public bool IsActive { get; set; } = true;

    public bool InterceptSystemButtons { get; set; } = false;

    public int LongPressTimeoutMs { get; set; } = 500;

    public int RepeatDelayMs { get; set; } = 500;

    public int RepeatRateMs { get; set; } = 100;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime LastSeenAt { get; set; } = DateTime.UtcNow;

    public ICollection<UsbRemoteButton> Buttons { get; set; } = new List<UsbRemoteButton>();
}