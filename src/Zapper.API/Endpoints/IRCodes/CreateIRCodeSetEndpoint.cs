using FastEndpoints;
using Zapper.Core.Models;
using Zapper.Services;

namespace Zapper.API.Endpoints.IRCodes;

public class CreateIRCodeSetEndpoint(IIRCodeService irCodeService) : Endpoint<IRCodeSet, IRCodeSet>
{
    public override void Configure()
    {
        Post("/api/ir-codes/sets");
        AllowAnonymous();
    }

    public override async Task HandleAsync(IRCodeSet req, CancellationToken ct)
    {
        var codeSet = await irCodeService.CreateCodeSetAsync(req);
        await SendCreatedAtAsync<GetIRCodeSetEndpoint>(new { Id = codeSet.Id }, codeSet, cancellation: ct);
    }
}