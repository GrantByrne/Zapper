using FastEndpoints;
using Zapper.Client.IRCodes;
using Zapper.Core.Models;
using Zapper.Services;

namespace Zapper.API.Endpoints.IRCodes;

public class SearchIrCodeSetsEndpoint(IIrCodeService irCodeService) : Endpoint<SearchIrCodeSetsRequest, IEnumerable<IrCodeSet>>
{
    public override void Configure()
    {
        Get("/api/ir-codes/sets/search");
        AllowAnonymous();
    }

    public override async Task HandleAsync(SearchIrCodeSetsRequest req, CancellationToken ct)
    {
        var codeSets = await irCodeService.SearchCodeSets(req.Brand, req.Model, (Zapper.Core.Models.DeviceType?)req.DeviceType);
        await SendOkAsync(codeSets, ct);
    }
}