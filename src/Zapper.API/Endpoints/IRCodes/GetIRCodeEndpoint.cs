using FastEndpoints;
using Zapper.Client.IRCodes;
using Zapper.Core.Models;
using Zapper.Services;

namespace Zapper.API.Endpoints.IRCodes;

public class GetIrCodeEndpoint(IIrCodeService irCodeService) : Endpoint<GetIrCodeRequest, IrCode?>
{
    public override void Configure()
    {
        Get("/api/ir-codes/sets/{codeSetId}/codes/{commandName}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get a specific IR code";
            s.Description = "Retrieve a specific IR code by command name from an IR code set. The command name could be 'Power', 'VolumeUp', 'Input_HDMI1', etc.";
            s.Responses[200] = "IR code found and returned successfully";
            s.Responses[404] = "IR code or code set not found";
            s.Responses[500] = "Internal server error";
        });
        Tags("IR Codes");
    }

    public override async Task HandleAsync(GetIrCodeRequest req, CancellationToken ct)
    {
        var code = await irCodeService.GetCode(req.CodeSetId, req.CommandName);
        if (code == null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        await SendOkAsync(code, ct);
    }
}