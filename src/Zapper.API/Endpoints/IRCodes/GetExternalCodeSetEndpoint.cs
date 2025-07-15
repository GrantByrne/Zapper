using FastEndpoints;
using Zapper.Core.Models;
using Zapper.Services;

namespace Zapper.API.Endpoints.IRCodes;

public class GetExternalCodeSetRequest
{
    public required string Manufacturer { get; set; }
    public required string DeviceType { get; set; }
    public required string Device { get; set; }
    public required string Subdevice { get; set; }
}

public class GetExternalCodeSetEndpoint(IExternalIrCodeService externalIrCodeService) : Endpoint<GetExternalCodeSetRequest, IrCodeSet>
{
    public override void Configure()
    {
        Get("/api/ir-codes/external/codeset/{manufacturer}/{deviceType}/{device}/{subdevice}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get IR code set from external database";
            s.Description = "Retrieves a complete IR code set for a specific device from the IRDB external database";
            s.Responses[200] = "IR code set with all available commands";
            s.Responses[404] = "Code set not found";
            s.Responses[503] = "External service unavailable";
        });
    }

    public override async Task HandleAsync(GetExternalCodeSetRequest req, CancellationToken ct)
    {
        var isAvailable = await externalIrCodeService.IsAvailableAsync();
        if (!isAvailable)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        var codeSet = await externalIrCodeService.GetCodeSetAsync(req.Manufacturer, req.DeviceType, req.Device, req.Subdevice);

        if (codeSet == null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        await SendOkAsync(codeSet, ct);
    }
}