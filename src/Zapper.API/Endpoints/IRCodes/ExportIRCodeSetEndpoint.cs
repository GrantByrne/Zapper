using FastEndpoints;
using Zapper.API.Models.Requests;
using Zapper.Services;

namespace Zapper.API.Endpoints.IRCodes;

public class ExportIrCodeSetEndpoint(IIrCodeService irCodeService) : Endpoint<ExportIrCodeSetRequest, string>
{
    public override void Configure()
    {
        Get("/api/ir-codes/sets/{id}/export");
        AllowAnonymous();
    }

    public override async Task HandleAsync(ExportIrCodeSetRequest req, CancellationToken ct)
    {
        try
        {
            var json = await irCodeService.ExportCodeSetAsync(req.Id);

            HttpContext.Response.ContentType = "application/json";
            HttpContext.Response.Headers["Content-Disposition"] = $"attachment; filename=\"ir-codes-{req.Id}.json\"";

            await SendStringAsync(json, cancellation: ct);
        }
        catch (ArgumentException ex)
        {
            AddError(ex.Message);
            await SendErrorsAsync(cancellation: ct);
        }
    }
}