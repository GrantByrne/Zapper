using FastEndpoints;
using Zapper.Client.IRCodes;
using Zapper.Services;

namespace Zapper.API.Endpoints.IRCodes;

public class InvalidateExternalCacheEndpoint(IExternalIrCodeService externalIrCodeService) : EndpointWithoutRequest<InvalidateExternalCacheResponse>
{
    public override void Configure()
    {
        Delete("/api/ir-codes/external/cache");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Invalidate external IR database cache";
            s.Description = "Clears all cached data from the external IR database to force fresh data on next request";
            s.Responses[200] = "Cache invalidated successfully";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await externalIrCodeService.InvalidateCache();

        await SendOkAsync(new InvalidateExternalCacheResponse
        {
            Success = true,
            Message = "External IR database cache has been invalidated"
        }, ct);
    }
}