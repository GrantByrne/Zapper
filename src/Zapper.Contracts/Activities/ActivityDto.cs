namespace Zapper.Contracts.Activities;

public class ActivityDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string Type { get; set; } = "Composite";
    public string? IconUrl { get; set; }
    public bool IsEnabled { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastUsed { get; set; }
    public ICollection<ActivityStepDto> Steps { get; set; } = new List<ActivityStepDto>();
}