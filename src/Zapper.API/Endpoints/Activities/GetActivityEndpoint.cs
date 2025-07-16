using FastEndpoints;
using Zapper.Client;
using Zapper.Client.Activities;
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
            s.Description = "Retrieves a specific activity by its unique identifier, including all associated steps and device commands.";
            s.Responses[200] = "Activity found and returned successfully";
            s.Responses[404] = "Activity not found with the specified ID";
            s.Responses[500] = "Internal server error";
        });
        Tags("Activities");
    }

    public override async Task HandleAsync(GetActivityRequest req, CancellationToken ct)
    {
        var activity = await activityService.GetActivityDto(req.Id);
        if (activity == null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        await SendOkAsync(activity, ct);
    }
}