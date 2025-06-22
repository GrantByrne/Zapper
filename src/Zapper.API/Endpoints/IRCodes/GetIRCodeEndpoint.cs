using FastEndpoints;
using Zapper.API.Models.Requests;
using Zapper.Core.Models;
using Zapper.Services;

namespace Zapper.API.Endpoints.IRCodes;

public class GetIRCodeEndpoint(IIRCodeService irCodeService) : Endpoint<GetIRCodeRequest, IRCode?>
{
    public override void Configure()
    {
        Get("/api/ir-codes/sets/{codeSetId}/codes/{commandName}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetIRCodeRequest req, CancellationToken ct)
    {
        var code = await irCodeService.GetCodeAsync(req.CodeSetId, req.CommandName);
        if (code == null)
        {
            await SendNotFoundAsync(ct);
            return;
        }
        
        await SendOkAsync(code, ct);
    }
}