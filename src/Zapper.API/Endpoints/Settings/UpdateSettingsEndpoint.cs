using FastEndpoints;
using Zapper.Services;

namespace Zapper.API.Endpoints.Settings;

public class UpdateSettingsEndpoint(ISettingsService settingsService) : Endpoint<UpdateSettingsRequest, UpdateSettingsResponse>
{
    public override void Configure()
    {
        Put("/api/settings");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Update application settings";
            s.Description = "Updates application settings and persists them to storage.";
            s.ExampleRequest = new UpdateSettingsRequest();
            s.Responses[200] = "Settings updated successfully";
            s.Responses[400] = "Invalid settings data";
            s.Responses[500] = "Internal server error";
        });
        Tags("Settings");
    }

    public override async Task HandleAsync(UpdateSettingsRequest req, CancellationToken ct)
    {
        try
        {
            await settingsService.UpdateSettingsAsync(req.Settings);

            await SendOkAsync(new UpdateSettingsResponse
            {
                Success = true,
                Message = "Settings updated successfully"
            }, ct);
        }
        catch (Exception ex)
        {
            await SendAsync(new UpdateSettingsResponse
            {
                Success = false,
                Message = $"Error updating settings: {ex.Message}"
            }, 500, ct);
        }
    }
}