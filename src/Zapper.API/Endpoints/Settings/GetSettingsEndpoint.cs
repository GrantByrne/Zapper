using FastEndpoints;
using Zapper.Core.Models;
using Zapper.Services;

namespace Zapper.API.Endpoints.Settings;

public class GetSettingsEndpoint(ISettingsService settingsService) : EndpointWithoutRequest<ZapperSettings>
{
    public override void Configure()
    {
        Get("/api/settings");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get application settings";
            s.Description = "Retrieves all application settings including general, device, network, hardware, and advanced configuration.";
            s.Responses[200] = "Settings retrieved successfully";
            s.Responses[500] = "Internal server error";
        });
        Tags("Settings");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            var settings = await settingsService.GetSettingsAsync();
            await SendOkAsync(settings, ct);
        }
        catch (Exception)
        {
            await SendAsync(new ZapperSettings(), 500, ct);
        }
    }
}