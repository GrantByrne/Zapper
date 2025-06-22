using FastEndpoints;
using Zapper.API.Models.Requests;
using Zapper.Services;

namespace Zapper.API.Endpoints.IRCodes;

public class DeleteIRCodeSetEndpoint(IIRCodeService irCodeService) : Endpoint<DeleteIRCodeSetRequest>
{
    public override void Configure()
    {
        Delete("/api/ir-codes/sets/{id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(DeleteIRCodeSetRequest req, CancellationToken ct)
    {
        await irCodeService.DeleteCodeSetAsync(req.Id);
        await SendNoContentAsync(ct);
    }
}