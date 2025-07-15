using FastEndpoints;
using Zapper.Contracts;
using Zapper.Services;

namespace Zapper.API.Endpoints.Activities;

public class CreateActivityEndpoint(IActivityService activityService) : Endpoint<CreateActivityRequest, ActivityDto>
{
    public override void Configure()
    {
        Post("/api/activities");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Create a new activity";
            s.Description = "Creates a new activity with the specified steps";
            s.Responses[201] = "Activity created successfully";
            s.Responses[400] = "Invalid request";
        });
    }

    public override async Task HandleAsync(CreateActivityRequest req, CancellationToken ct)
    {
        var activity = await activityService.CreateActivity(req);
        await SendCreatedAtAsync<GetActivityEndpoint>(new { id = activity.Id }, activity, cancellation: ct);
    }
}