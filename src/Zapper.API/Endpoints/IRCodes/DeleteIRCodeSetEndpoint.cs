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
        Summary(s =>
        {
            s.Summary = "Delete an IR code set";
            s.Description = "Permanently delete an IR code set and all its associated IR codes. This action cannot be undone.";
            s.Responses[204] = "IR code set deleted successfully";
            s.Responses[404] = "IR code set not found";
            s.Responses[500] = "Internal server error";
        });
        Tags("IR Codes");
    }

    public override async Task HandleAsync(DeleteIrCodeSetRequest req, CancellationToken ct)
    {
        await irCodeService.DeleteCodeSet(req.Id);
        await SendNoContentAsync(ct);
    }
}