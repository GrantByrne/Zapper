using System.ComponentModel.DataAnnotations;

namespace Zapper.Core.Models;

public class Activity
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public string? IconUrl { get; set; }

    public bool IsEnabled { get; set; } = true;

    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime LastUsed { get; set; } = DateTime.UtcNow;

    public ICollection<ActivityDevice> ActivityDevices { get; set; } = new List<ActivityDevice>();

    public ICollection<ActivityStep> Steps { get; set; } = new List<ActivityStep>();
}