using Zapper.Models;

namespace Zapper.Services;

public interface IActivityService
{
    Task<IEnumerable<Activity>> GetAllActivitiesAsync();
    Task<Activity?> GetActivityAsync(int id);
    Task<Activity> CreateActivityAsync(Activity activity);
    Task<Activity?> UpdateActivityAsync(int id, Activity activity);
    Task<bool> DeleteActivityAsync(int id);
    Task<bool> ExecuteActivityAsync(int activityId, CancellationToken cancellationToken = default);
    Task<bool> StopActivityAsync(int activityId, CancellationToken cancellationToken = default);
    Task<Activity?> AddDeviceToActivityAsync(int activityId, int deviceId, bool isPrimary = false);
    Task<bool> RemoveDeviceFromActivityAsync(int activityId, int deviceId);
    Task<ActivityStep?> AddStepToActivityAsync(int activityId, int deviceCommandId, int stepOrder, int delayBeforeMs = 0, int delayAfterMs = 0);
    Task<bool> RemoveStepFromActivityAsync(int activityId, int stepId);
    Task<bool> ReorderActivityStepsAsync(int activityId, int[] stepIds);
}