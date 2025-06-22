using FastEndpoints;
using Zapper.Core.Models;
using Zapper.Services;

namespace Zapper.Endpoints.Activities;

public class GetAllActivitiesEndpoint(IActivityService activityService) : EndpointWithoutRequest<IEnumerable<Activity>>
{
    public override void Configure()
    {
        Get("/api/activities");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get all activities";
            s.Description = "Retrieve a list of all configured activities";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var activities = await activityService.GetAllActivitiesAsync();
        await SendOkAsync(activities, ct);
    }
}