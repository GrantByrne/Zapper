using FastEndpoints;
using Zapper.Services;

namespace Zapper.API.Endpoints.IRCodes;

public class SeedDefaultCodesEndpoint(IIrCodeService irCodeService) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Post("/api/ir-codes/seed");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await irCodeService.SeedDefaultCodesAsync();
        await SendOkAsync("Default IR codes seeded successfully", ct);
    }
}