using FastEndpoints;
using Zapper.API.Models.Requests;
using Zapper.Services;

namespace Zapper.API.Endpoints.IRCodes;

public class DeleteIrCodeSetEndpoint(IIrCodeService irCodeService) : Endpoint<DeleteIrCodeSetRequest>
{
    public override void Configure()
    {
        Delete("/api/ir-codes/sets/{id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(DeleteIrCodeSetRequest req, CancellationToken ct)
    {
        await irCodeService.DeleteCodeSetAsync(req.Id);
        await SendNoContentAsync(ct);
    }
}