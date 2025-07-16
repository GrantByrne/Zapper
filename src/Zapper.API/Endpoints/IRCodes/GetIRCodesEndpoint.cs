using FastEndpoints;
using Zapper.Core.Models;
using Zapper.Services;
using Zapper.Client.Devices;
using Zapper.Client.IRCodes;
using Zapper.Client.Settings;
using Zapper.Client.System;
using Zapper.Client.UsbRemotes;

namespace Zapper.API.Endpoints.IRCodes;

public class GetIrCodesEndpoint(IIrCodeService irCodeService) : Endpoint<GetIrCodesRequest, IEnumerable<IrCode>>
{
    public override void Configure()
    {
        Get("/api/ir-codes/sets/{codeSetId}/codes");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get all IR codes in a set";
            s.Description = "Retrieve all IR codes belonging to a specific IR code set. Each code represents a command that can be sent to the device.";
            s.Responses[200] = "List of IR codes retrieved successfully";
            s.Responses[404] = "IR code set not found";
            s.Responses[500] = "Internal server error";
        });
        Tags("IR Codes");
    }

    public override async Task HandleAsync(GetIrCodesRequest req, CancellationToken ct)
    {
        var codes = await irCodeService.GetCodes(req.CodeSetId);
        await SendOkAsync(codes, ct);
    }
}