using FastEndpoints;
using Zapper.Services;

namespace Zapper.API.Endpoints.Activities;

public class ExecuteActivityEndpoint(IActivityService activityService) : Endpoint<ExecuteActivityRequest, ExecuteActivityResponse>
{
    public override void Configure()
    {
        Post("/api/activities/{id}/execute");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Execute an activity";
            s.Description = "Execute all steps in an activity sequence";
        });
    }

    public override async Task HandleAsync(ExecuteActivityRequest req, CancellationToken ct)
    {
        var success = await activityService.ExecuteActivityAsync(req.Id, ct);
        if (!success)
        {
            await SendAsync(new ExecuteActivityResponse
            {
                Message = $"Failed to execute activity {req.Id}"
            }, 400, ct);
            return;
        }

        await SendOkAsync(new ExecuteActivityResponse
        {
            Message = "Activity executed successfully"
        }, ct);
    }
}