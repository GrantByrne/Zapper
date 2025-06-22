namespace Zapper.Core.Models;

public class ActivityDevice
{
    public int Id { get; set; }
    
    public int ActivityId { get; set; }
    
    public int DeviceId { get; set; }
    
    public bool IsPrimaryDevice { get; set; }
    
    public Activity Activity { get; set; } = null!;
    
    public Device Device { get; set; } = null!;
}