using FastEndpoints;
using Zapper.Core.Models;
using Zapper.Services;

namespace Zapper.API.Endpoints.IRCodes;

public class CreateIrCodeSetEndpoint(IIrCodeService irCodeService) : Endpoint<IrCodeSet, IrCodeSet>
{
    public override void Configure()
    {
        Post("/api/ir-codes/sets");
        AllowAnonymous();
    }

    public override async Task HandleAsync(IrCodeSet req, CancellationToken ct)
    {
        var codeSet = await irCodeService.CreateCodeSetAsync(req);
        await SendCreatedAtAsync<GetIrCodeSetEndpoint>(new { Id = codeSet.Id }, codeSet, cancellation: ct);
    }
}