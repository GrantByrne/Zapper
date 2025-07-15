using FastEndpoints;
using Zapper.Core.Models;
using Zapper.Services;
using Zapper.API.Models.Requests;

namespace Zapper.API.Endpoints.IRCodes;

public class GetIrCodeSetEndpoint(IIrCodeService irCodeService) : Endpoint<GetIrCodeSetRequest, IrCodeSet?>
{
    public override void Configure()
    {
        Get("/api/ir-codes/sets/{id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetIrCodeSetRequest req, CancellationToken ct)
    {
        var codeSet = await irCodeService.GetCodeSetAsync(req.Id);
        if (codeSet == null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        await SendOkAsync(codeSet, ct);
    }
}