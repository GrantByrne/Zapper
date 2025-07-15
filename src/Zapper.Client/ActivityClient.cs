using Zapper.Client.Abstractions;
using Zapper.Contracts.Activities;

namespace Zapper.Client;

/// <summary>
/// Implementation of activity client using Refit
/// </summary>
public class ActivityClient(IActivityApi activityApi) : IActivityClient
{
    public async Task<IEnumerable<ActivityDto>> GetAllActivitiesAsync(CancellationToken cancellationToken = default)
    {
        return await activityApi.GetAllActivitiesAsync(cancellationToken);
    }

    public async Task<ExecuteActivityResponse> ExecuteActivityAsync(int id, CancellationToken cancellationToken = default)
    {
        return await activityApi.ExecuteActivityAsync(id, cancellationToken);
    }
}