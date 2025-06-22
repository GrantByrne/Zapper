namespace Zapper.Core.Models;

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