using FastEndpoints;
using Zapper.Contracts;
using Zapper.Contracts.Activities;
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
            s.Description = "Updates an existing activity including its name, description, type, enabled status, and steps. All fields will be updated with the provided values.";
            s.ExampleRequest = new UpdateActivityRequest
            {
                Id = 1,
                Name = "Watch Movie Updated",
                Description = "Updated sequence for movie watching",
                Type = "Scene",
                IsEnabled = true,
                Steps = new List<UpdateActivityStepRequest>
                {
                    new() { Id = 1, DeviceId = 1, Command = "Power", DelayMs = 1000, SortOrder = 1 },
                    new() { Id = 2, DeviceId = 2, Command = "Power", DelayMs = 500, SortOrder = 2 },
                    new() { DeviceId = 1, Command = "Input_HDMI2", DelayMs = 500, SortOrder = 3 }
                }
            };
            s.Responses[200] = "Activity updated successfully";
            s.Responses[400] = "Invalid request - validation errors";
            s.Responses[404] = "Activity not found";
            s.Responses[500] = "Internal server error";
        });
        Tags("Activities");
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