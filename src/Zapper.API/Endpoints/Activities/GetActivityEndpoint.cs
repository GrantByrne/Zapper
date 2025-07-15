using FastEndpoints;
using Zapper.Contracts;
using Zapper.Services;

namespace Zapper.API.Endpoints.Activities;

public class GetActivityEndpoint(IActivityService activityService) : Endpoint<GetActivityRequest, ActivityDto>
{
    public override void Configure()
    {
        Get("/api/activities/{id}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get activity by ID";
            s.Description = "Retrieves the specified activity by ID";
            s.Responses[200] = "Activity found";
            s.Responses[404] = "Activity not found";
        });
    }

    public override async Task HandleAsync(GetActivityRequest req, CancellationToken ct)
    {
        var activity = await activityService.GetActivity(req.Id);
        if (activity == null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        await SendOkAsync(activity, ct);
    }
}