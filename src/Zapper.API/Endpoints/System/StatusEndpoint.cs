using FastEndpoints;

namespace Zapper.API.Endpoints.System;

public class StatusEndpoint : EndpointWithoutRequest<StatusResponse>
{
    public override void Configure()
    {
        Get("/api/system/status");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get system status";
            s.Description = "Returns current system status, health information, and basic statistics";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var response = new StatusResponse
        {
            ConnectedClients = 0
        };

        await SendOkAsync(response, ct);
    }
}