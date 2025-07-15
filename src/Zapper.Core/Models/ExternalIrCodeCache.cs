using System.ComponentModel.DataAnnotations;

namespace Zapper.Core.Models;

public class ExternalIrCodeCache
{
    public int Id { get; set; }

    [Required]
    public string CacheKey { get; set; } = string.Empty;

    [Required]
    public string CachedData { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime ExpiresAt { get; set; }
}