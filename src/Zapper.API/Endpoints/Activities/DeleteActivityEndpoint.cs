using FastEndpoints;
using Zapper.Contracts;
using Zapper.Services;

namespace Zapper.API.Endpoints.Activities;

public class DeleteActivityEndpoint(IActivityService activityService) : Endpoint<DeleteActivityRequest, EmptyResponse>
{
    public override void Configure()
    {
        Delete("/api/activities/{id}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Delete an activity";
            s.Description = "Deletes the specified activity by ID";
            s.Responses[204] = "Activity deleted successfully";
            s.Responses[404] = "Activity not found";
        });
    }

    public override async Task HandleAsync(DeleteActivityRequest req, CancellationToken ct)
    {
        await activityService.DeleteActivity(req.Id);
        await SendNoContentAsync(ct);
    }
}