using FastEndpoints;
using Zapper.Core.Models;
using Zapper.Services;

namespace Zapper.API.Endpoints.Activities;

public class GetAllActivitiesEndpoint(IActivityService activityService) : EndpointWithoutRequest<IEnumerable<Activity>>
{
    public override void Configure()
    {
        Get("/api/activities");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get all activities";
            s.Description = "Retrieve a list of all configured activities in the system. Activities are sequences of device commands that can be executed together to control multiple devices.";
            s.Responses[200] = "List of activities retrieved successfully";
            s.Responses[500] = "Internal server error";
        });
        Tags("Activities");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var activities = await activityService.GetAllActivities();
        await SendOkAsync(activities, ct);
    }
}