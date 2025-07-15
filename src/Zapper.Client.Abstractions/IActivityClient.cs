using Zapper.Contracts.Activities;

namespace Zapper.Client.Abstractions;

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
}