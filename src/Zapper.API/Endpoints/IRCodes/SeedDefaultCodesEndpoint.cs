using FastEndpoints;
using Zapper.Services;

namespace Zapper.API.Endpoints.IRCodes;

public class SeedDefaultCodesEndpoint(IIrCodeService irCodeService) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Post("/api/ir-codes/seed");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Seed default IR codes";
            s.Description = "Seeds the database with a default set of common IR codes for popular devices. This is useful for initial setup or testing.";
            s.Responses[200] = "Default IR codes seeded successfully";
            s.Responses[500] = "Internal server error";
        });
        Tags("IR Codes", "Admin");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await irCodeService.SeedDefaultCodes();
        await SendOkAsync("Default IR codes seeded successfully", ct);
    }
}