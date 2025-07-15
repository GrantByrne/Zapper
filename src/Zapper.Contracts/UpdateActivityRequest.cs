namespace Zapper.Contracts;

public class UpdateActivityRequest
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Type { get; set; } = "Composite";
    public bool IsEnabled { get; set; } = true;
    public List<UpdateActivityStepRequest> Steps { get; set; } = [];
}

public class UpdateActivityStepRequest
{
    public int? Id { get; set; }
    public int DeviceId { get; set; }
    public string Command { get; set; } = string.Empty;
    public int DelayMs { get; set; } = 500;
    public int SortOrder { get; set; }
}