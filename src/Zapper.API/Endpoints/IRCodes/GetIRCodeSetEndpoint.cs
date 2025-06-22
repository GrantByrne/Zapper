using FastEndpoints;
using Zapper.Core.Models;
using Zapper.Services;
using Zapper.API.Models.Requests;

namespace Zapper.API.Endpoints.IRCodes;

public class GetIRCodeSetEndpoint(IIRCodeService irCodeService) : Endpoint<GetIRCodeSetRequest, IRCodeSet?>
{
    public override void Configure()
    {
        Get("/api/ir-codes/sets/{id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetIRCodeSetRequest req, CancellationToken ct)
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