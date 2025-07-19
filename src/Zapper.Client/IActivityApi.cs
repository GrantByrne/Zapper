using Refit;
using Zapper.Client.Activities;

namespace Zapper.Client;

/// <summary>
/// Refit interface for activity API endpoints
/// </summary>
public interface IActivityApi
{
    /// <summary>
    /// Get all activities
    /// </summary>
    [Get(ApiRoutes.Activities.GetAll)]
    Task<IEnumerable<ActivityDto>> GetAllActivitiesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute an activity
    /// </summary>
    [Post(ApiRoutes.Activities.Execute)]
    Task<ExecuteActivityResponse> ExecuteActivityAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new activity
    /// </summary>
    [Post(ApiRoutes.Activities.Create)]
    Task<ActivityDto> CreateActivityAsync([Body] CreateActivityRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete an activity
    /// </summary>
    [Delete(ApiRoutes.Activities.Delete)]
    Task DeleteActivityAsync(int id, CancellationToken cancellationToken = default);
}