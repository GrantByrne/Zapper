using FastEndpoints;
using Zapper.Core.Models;
using Zapper.Services;
using Zapper.API.Models.Requests;

namespace Zapper.API.Endpoints.IRCodes;

public class SearchIRCodeSetsEndpoint(IIRCodeService irCodeService) : Endpoint<SearchIRCodeSetsRequest, IEnumerable<IRCodeSet>>
{
    public override void Configure()
    {
        Get("/api/ir-codes/sets/search");
        AllowAnonymous();
    }

    public override async Task HandleAsync(SearchIRCodeSetsRequest req, CancellationToken ct)
    {
        var codeSets = await irCodeService.SearchCodeSetsAsync(req.Brand, req.Model, req.DeviceType);
        await SendOkAsync(codeSets, ct);
    }
}