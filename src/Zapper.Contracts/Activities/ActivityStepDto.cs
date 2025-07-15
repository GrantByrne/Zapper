namespace Zapper.Contracts.Activities;

public class ActivityStepDto
{
    public int Id { get; set; }
    public int ActivityId { get; set; }
    public int DeviceId { get; set; }
    public string Command { get; set; } = string.Empty;
    public int DelayMs { get; set; }
    public int SortOrder { get; set; }
    public string? DeviceName { get; set; }
}