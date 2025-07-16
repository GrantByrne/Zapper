using Zapper.Client;
using Zapper.Client.Activities;

namespace Zapper.Client;

/// <summary>
/// Client interface for activity management operations
/// </summary>
public interface IActivityClient
{
    /// <summary>
    /// Get all activities
    /// </summary>
    Task<IEnumerable<ActivityDto>> GetAllActivitiesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute an activity
    /// </summary>
    Task<ExecuteActivityResponse> ExecuteActivityAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new activity
    /// </summary>
    Task<ActivityDto> CreateActivityAsync(CreateActivityRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete an activity
    /// </summary>
    Task DeleteActivityAsync(int id, CancellationToken cancellationToken = default);
}