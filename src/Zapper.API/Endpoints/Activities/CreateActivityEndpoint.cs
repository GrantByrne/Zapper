using FastEndpoints;
using Zapper.Client;
using Zapper.Client.Activities;
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
            s.Description = "Creates a new activity with the specified steps. Activities are sequences of device commands that can be executed together.";
            s.ExampleRequest = new CreateActivityRequest
            {
                Name = "Watch Movie",
                Description = "Turn on TV, sound system, and set to correct input",
                Type = "Scene",
                IsEnabled = true,
                Steps = new List<CreateActivityStepRequest>
                {
                    new() { DeviceId = 1, Command = "Power", DelayMs = 1000, SortOrder = 1 },
                    new() { DeviceId = 2, Command = "Power", DelayMs = 500, SortOrder = 2 },
                    new() { DeviceId = 1, Command = "Input_HDMI1", DelayMs = 500, SortOrder = 3 }
                }
            };
            s.Responses[201] = "Activity created successfully";
            s.Responses[400] = "Invalid request - validation errors";
            s.Responses[500] = "Internal server error";
        });
        Tags("Activities");
    }

    public override async Task HandleAsync(CreateActivityRequest req, CancellationToken ct)
    {
        var activity = await activityService.CreateActivity(req);
        await SendCreatedAtAsync<GetActivityEndpoint>(new { id = activity.Id }, activity, cancellation: ct);
    }
}