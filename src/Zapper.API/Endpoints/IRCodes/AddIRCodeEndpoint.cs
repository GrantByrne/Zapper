using FastEndpoints;
using Zapper.Contracts.Devices;
using Zapper.Contracts.IRCodes;
using Zapper.Contracts.Settings;
using Zapper.Contracts.System;
using Zapper.Contracts.UsbRemotes;
using Zapper.Core.Models;
using Zapper.Services;

namespace Zapper.API.Endpoints.IRCodes;

public class AddIrCodeEndpoint(IIrCodeService irCodeService) : Endpoint<AddIrCodeRequest, IrCode>
{
    public override void Configure()
    {
        Post("/api/ir-codes/sets/{codeSetId}/codes");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Add an IR code to a set";
            s.Description = "Add a new IR code to an existing IR code set. The code data should be in Pronto hex format or raw IR timing data.";
            s.ExampleRequest = new AddIrCodeRequest
            {
                CodeSetId = 1,
                Code = new IrCode
                {
                    CommandName = "Power",
                    Protocol = "NEC",
                    Frequency = 38000,
                    HexCode = "0000 006D 0022 0002 0155 00AA..."
                }
            };
            s.Responses[200] = "IR code added successfully";
            s.Responses[400] = "Invalid request - validation errors or duplicate command name";
            s.Responses[404] = "IR code set not found";
            s.Responses[500] = "Internal server error";
        });
        Tags("IR Codes");
    }

    public override async Task HandleAsync(AddIrCodeRequest req, CancellationToken ct)
    {
        try
        {
            var code = await irCodeService.AddCode(req.CodeSetId, req.Code);
            await SendOkAsync(code, ct);
        }
        catch (ArgumentException ex)
        {
            AddError(ex.Message);
            await SendErrorsAsync(cancellation: ct);
        }
    }
}