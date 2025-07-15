using FastEndpoints;
using Zapper.Core.Models;
using Zapper.Services;

namespace Zapper.API.Endpoints.IRCodes;

public class GetIrCodeSetsEndpoint(IIrCodeService irCodeService) : EndpointWithoutRequest<IEnumerable<IrCodeSet>>
{
    public override void Configure()
    {
        Get("/api/ir-codes/sets");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get all IR code sets";
            s.Description = "Retrieve a list of all available IR code sets. Each set contains IR codes for controlling a specific device brand and model.";
            s.Responses[200] = "List of IR code sets retrieved successfully";
            s.Responses[500] = "Internal server error";
        });
        Tags("IR Codes");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var codeSets = await irCodeService.GetCodeSets();
        await SendOkAsync(codeSets, ct);
    }
}