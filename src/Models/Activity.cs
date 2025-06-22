using System.ComponentModel.DataAnnotations;

namespace ZapperHub.Models;

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

public class ActivityDevice
{
    public int Id { get; set; }
    
    public int ActivityId { get; set; }
    
    public int DeviceId { get; set; }
    
    public bool IsPrimaryDevice { get; set; }
    
    public Activity Activity { get; set; } = null!;
    
    public Device Device { get; set; } = null!;
}

public class ActivityStep
{
    public int Id { get; set; }
    
    public int ActivityId { get; set; }
    
    public int DeviceCommandId { get; set; }
    
    public int StepOrder { get; set; }
    
    public int DelayBeforeMs { get; set; } = 0;
    
    public int DelayAfterMs { get; set; } = 0;
    
    public bool IsRequired { get; set; } = true;
    
    public Activity Activity { get; set; } = null!;
    
    public DeviceCommand DeviceCommand { get; set; } = null!;
}