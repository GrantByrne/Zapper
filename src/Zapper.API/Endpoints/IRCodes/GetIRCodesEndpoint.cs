using FastEndpoints;
using Zapper.Core.Models;
using Zapper.Services;
using Zapper.API.Models.Requests;

namespace Zapper.API.Endpoints.IRCodes;

public class GetIRCodesEndpoint(IIRCodeService irCodeService) : Endpoint<GetIRCodesRequest, IEnumerable<IRCode>>
{
    public override void Configure()
    {
        Get("/api/ir-codes/sets/{codeSetId}/codes");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetIRCodesRequest req, CancellationToken ct)
    {
        var codes = await irCodeService.GetCodesAsync(req.CodeSetId);
        await SendOkAsync(codes, ct);
    }
}