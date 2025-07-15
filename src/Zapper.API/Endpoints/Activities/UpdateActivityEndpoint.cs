using FastEndpoints;
using Zapper.Contracts;
using Zapper.Services;

namespace Zapper.API.Endpoints.Activities;

public class UpdateActivityEndpoint(IActivityService activityService) : Endpoint<UpdateActivityRequest, ActivityDto>
{
    public override void Configure()
    {
        Put("/api/activities/{id}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Update an activity";
            s.Description = "Updates the specified activity";
            s.Responses[200] = "Activity updated successfully";
            s.Responses[404] = "Activity not found";
        });
    }

    public override async Task HandleAsync(UpdateActivityRequest req, CancellationToken ct)
    {
        var activity = await activityService.UpdateActivity(req);
        if (activity == null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        await SendOkAsync(activity, ct);
    }
}