using FastEndpoints;
using Zapper.Core.Models;
using Zapper.Services;

namespace Zapper.API.Endpoints.IRCodes;

public class GetIRCodeSetsEndpoint(IIRCodeService irCodeService) : EndpointWithoutRequest<IEnumerable<IRCodeSet>>
{
    public override void Configure()
    {
        Get("/api/ir-codes/sets");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var codeSets = await irCodeService.GetCodeSetsAsync();
        await SendOkAsync(codeSets, ct);
    }
}