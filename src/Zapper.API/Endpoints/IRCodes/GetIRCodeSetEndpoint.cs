using FastEndpoints;
using Zapper.Core.Models;
using Zapper.Services;
using Zapper.Client.IRCodes;

namespace Zapper.API.Endpoints.IRCodes;

public class GetIrCodeSetEndpoint(IIrCodeService irCodeService) : Endpoint<GetIrCodeSetRequest, IrCodeSet?>
{
    public override void Configure()
    {
        Get("/api/ir-codes/sets/{id}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get IR code set by ID";
            s.Description = "Retrieve a specific IR code set by its unique identifier, including all associated IR codes.";
            s.Responses[200] = "IR code set found and returned successfully";
            s.Responses[404] = "IR code set not found with the specified ID";
            s.Responses[500] = "Internal server error";
        });
        Tags("IR Codes");
    }

    public override async Task HandleAsync(GetIrCodeSetRequest req, CancellationToken ct)
    {
        var codeSet = await irCodeService.GetCodeSet(req.Id);
        if (codeSet == null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        await SendOkAsync(codeSet, ct);
    }
}