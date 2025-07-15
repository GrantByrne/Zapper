using FastEndpoints;
using Zapper.Core.Models;
using Zapper.Services;

namespace Zapper.API.Endpoints.System;

public class GetSystemInfoEndpoint(IIrTroubleshootingService troubleshootingService) : EndpointWithoutRequest<SystemInfoResult>
{
    public override void Configure()
    {
        Get("/api/system/info");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get system information";
            s.Description = "Retrieves detailed system information including platform, GPIO support, and hardware compatibility.";
            s.Responses[200] = "System information retrieved successfully";
            s.Responses[500] = "Internal server error";
        });
        Tags("System");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await troubleshootingService.GetSystemInfoAsync();
        await SendOkAsync(result, ct);
    }
}