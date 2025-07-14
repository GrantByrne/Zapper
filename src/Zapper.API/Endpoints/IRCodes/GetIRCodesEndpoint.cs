using FastEndpoints;
using Zapper.Core.Models;
using Zapper.Services;
using Zapper.API.Models.Requests;

namespace Zapper.API.Endpoints.IRCodes;

public class GetIrCodesEndpoint(IIrCodeService irCodeService) : Endpoint<GetIrCodesRequest, IEnumerable<IrCode>>
{
    public override void Configure()
    {
        Get("/api/ir-codes/sets/{codeSetId}/codes");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetIrCodesRequest req, CancellationToken ct)
    {
        var codes = await irCodeService.GetCodesAsync(req.CodeSetId);
        await SendOkAsync(codes, ct);
    }
}