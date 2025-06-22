using Zapper.Core.Models;

namespace Zapper.Core.Interfaces;

public interface IActivityManager
{
    Task<bool> ExecuteActivityAsync(Activity activity);
    Task<ActivityExecutionResult> ExecuteActivityWithResultAsync(Activity activity);
    Task<IEnumerable<Activity>> GetActivitiesAsync();
    Task<Activity?> GetActivityByIdAsync(int id);
}

public class ActivityExecutionResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public TimeSpan ExecutionTime { get; set; }
    public List<StepResult> StepResults { get; set; } = new();
}

public class StepResult
{
    public int StepId { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public TimeSpan ExecutionTime { get; set; }
}