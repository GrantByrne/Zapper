using FastEndpoints;
using Zapper.Core.Models;
using Zapper.Services;

namespace Zapper.API.Endpoints.IRCodes;

public class CreateIrCodeSetEndpoint(IIrCodeService irCodeService) : Endpoint<IrCodeSet, IrCodeSet>
{
    public override void Configure()
    {
        Post("/api/ir-codes/sets");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Create a new IR code set";
            s.Description = "Create a new IR code set for a specific device brand and model. The set can then be populated with individual IR codes.";
            s.ExampleRequest = new IrCodeSet
            {
                Brand = "Samsung",
                Model = "UN55MU8000",
                DeviceType = DeviceType.Television,
                Description = "Custom IR codes for Samsung TV"
            };
            s.Responses[201] = "IR code set created successfully";
            s.Responses[400] = "Invalid request - validation errors";
            s.Responses[500] = "Internal server error";
        });
        Tags("IR Codes");
    }

    public override async Task HandleAsync(IrCodeSet req, CancellationToken ct)
    {
        var codeSet = await irCodeService.CreateCodeSet(req);
        await SendCreatedAtAsync<GetIrCodeSetEndpoint>(new { Id = codeSet.Id }, codeSet, cancellation: ct);
    }
}