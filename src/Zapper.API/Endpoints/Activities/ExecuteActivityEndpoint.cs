using FastEndpoints;
using Zapper.Client.Activities;
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
            s.Description = "Execute all steps in an activity sequence. This will run all configured device commands in the specified order with the defined delays between commands.";
            s.ExampleRequest = new ExecuteActivityRequest { Id = 1 };
            s.Responses[200] = "Activity executed successfully";
            s.Responses[400] = "Failed to execute activity - invalid activity ID or execution error";
            s.Responses[404] = "Activity not found";
            s.Responses[500] = "Internal server error";
        });
        Tags("Activities");
    }

    public override async Task HandleAsync(ExecuteActivityRequest req, CancellationToken ct)
    {
        var success = await activityService.ExecuteActivity(req.Id, ct);
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