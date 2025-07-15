using Zapper.Core.Models;

namespace Zapper.Core.Interfaces;

public interface IActivityManager
{
    Task<bool> ExecuteActivityAsync(Activity activity);
    Task<ActivityExecutionResult> ExecuteActivityWithResultAsync(Activity activity);
    Task<IEnumerable<Activity>> GetActivitiesAsync();
    Task<Activity?> GetActivityByIdAsync(int id);
}