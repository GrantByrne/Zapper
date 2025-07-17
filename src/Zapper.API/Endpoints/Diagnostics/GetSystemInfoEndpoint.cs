using FastEndpoints;
using Zapper.Core.Models;
using Zapper.Services;

namespace Zapper.API.Endpoints.Diagnostics;

public class GetSystemInfoEndpoint(ISystemDiagnosticsService diagnosticsService) : EndpointWithoutRequest<SystemInfo>
{
    public override void Configure()
    {
        Get("/api/diagnostics/system-info");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var info = await diagnosticsService.GetSystemInfoAsync();
        await SendOkAsync(info, ct);
    }
}