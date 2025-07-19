// using Zapper.Client; // No need to reference own namespace

using Zapper.Client.Activities;

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

    public async Task<ActivityDto> CreateActivityAsync(CreateActivityRequest request, CancellationToken cancellationToken = default)
    {
        return await activityApi.CreateActivityAsync(request, cancellationToken);
    }

    public async Task DeleteActivityAsync(int id, CancellationToken cancellationToken = default)
    {
        await activityApi.DeleteActivityAsync(id, cancellationToken);
    }
}