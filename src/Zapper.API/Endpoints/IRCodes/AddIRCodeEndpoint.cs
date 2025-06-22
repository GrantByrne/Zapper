using FastEndpoints;
using Zapper.API.Models.Requests;
using Zapper.Core.Models;
using Zapper.Services;

namespace Zapper.API.Endpoints.IRCodes;

public class AddIRCodeEndpoint(IIRCodeService irCodeService) : Endpoint<AddIRCodeRequest, IRCode>
{
    public override void Configure()
    {
        Post("/api/ir-codes/sets/{codeSetId}/codes");
        AllowAnonymous();
    }

    public override async Task HandleAsync(AddIRCodeRequest req, CancellationToken ct)
    {
        try
        {
            var code = await irCodeService.AddCodeAsync(req.CodeSetId, req.Code);
            await SendOkAsync(code, ct);
        }
        catch (ArgumentException ex)
        {
            AddError(ex.Message);
            await SendErrorsAsync(cancellation: ct);
        }
    }
}