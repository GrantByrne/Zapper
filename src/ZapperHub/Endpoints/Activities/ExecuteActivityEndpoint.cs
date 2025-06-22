using FastEndpoints;
using ZapperHub.Services;

namespace ZapperHub.Endpoints.Activities;

public class ExecuteActivityRequest
{
    public int Id { get; set; }
}

public class ExecuteActivityResponse
{
    public string Message { get; set; } = string.Empty;
}

public class ExecuteActivityEndpoint : Endpoint<ExecuteActivityRequest, ExecuteActivityResponse>
{
    public IActivityService ActivityService { get; set; } = null!;

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
        var success = await ActivityService.ExecuteActivityAsync(req.Id, ct);
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