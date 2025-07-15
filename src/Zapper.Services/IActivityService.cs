using Zapper.Core.Models;
using Zapper.Contracts;
using Zapper.Contracts.Activities;

namespace Zapper.Services;

public interface IActivityService
{
    Task<IEnumerable<Activity>> GetAllActivities();
    Task<Activity?> GetActivity(int id);
    Task<Zapper.Contracts.Activities.ActivityDto?> GetActivityDto(int id);
    Task<Activity> CreateActivity(Activity activity);
    Task<Zapper.Contracts.Activities.ActivityDto> CreateActivity(CreateActivityRequest request);
    Task<Activity?> UpdateActivity(int id, Activity activity);
    Task<Zapper.Contracts.Activities.ActivityDto?> UpdateActivity(UpdateActivityRequest request);
    Task<bool> DeleteActivity(int id);
    Task<bool> ExecuteActivity(int activityId, CancellationToken cancellationToken = default);
    Task<bool> StopActivity(int activityId, CancellationToken cancellationToken = default);
    Task<Activity?> AddDeviceToActivity(int activityId, int deviceId, bool isPrimary = false);
    Task<bool> RemoveDeviceFromActivity(int activityId, int deviceId);
    Task<ActivityStep?> AddStepToActivity(int activityId, int deviceCommandId, int stepOrder, int delayBeforeMs = 0, int delayAfterMs = 0);
    Task<bool> RemoveStepFromActivity(int activityId, int stepId);
    Task<bool> ReorderActivitySteps(int activityId, int[] stepIds);
}