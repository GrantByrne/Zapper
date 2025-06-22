using FastEndpoints;
using Zapper.Core.Models;
using Zapper.Services;

namespace Zapper.Endpoints.Activities;

public class GetAllActivitiesEndpoint : EndpointWithoutRequest<IEnumerable<Activity>>
{
    public IActivityService ActivityService { get; set; } = null!;

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
        var activities = await ActivityService.GetAllActivitiesAsync();
        await SendOkAsync(activities, ct);
    }
}