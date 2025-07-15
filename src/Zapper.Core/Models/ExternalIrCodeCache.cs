using System.ComponentModel.DataAnnotations;

namespace Zapper.Core.Models;

public class ExternalIrCodeCache
{
    public int Id { get; set; }

    [Required]
    public string CacheKey { get; set; } = "";

    [Required]
    public string CachedData { get; set; } = "";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime ExpiresAt { get; set; }
}