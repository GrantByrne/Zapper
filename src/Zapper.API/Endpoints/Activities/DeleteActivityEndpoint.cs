using FastEndpoints;
using Zapper.Client;
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
            s.Description = "Permanently deletes the specified activity and all its associated steps. This action cannot be undone.";
            s.Responses[204] = "Activity deleted successfully";
            s.Responses[404] = "Activity not found";
            s.Responses[500] = "Internal server error";
        });
        Tags("Activities");
    }

    public override async Task HandleAsync(DeleteActivityRequest req, CancellationToken ct)
    {
        await activityService.DeleteActivity(req.Id);
        await SendNoContentAsync(ct);
    }
}