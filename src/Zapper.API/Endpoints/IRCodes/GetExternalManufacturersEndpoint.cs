using FastEndpoints;
using Zapper.Services;

namespace Zapper.API.Endpoints.IRCodes;

public class GetExternalManufacturersEndpoint(IExternalIrCodeService externalIrCodeService) : EndpointWithoutRequest<IEnumerable<string>>
{
    public override void Configure()
    {
        Get("/api/ir-codes/external/manufacturers");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get available manufacturers from external IR database";
            s.Description = "Retrieves a list of all available manufacturers from the IRDB external database";
            s.Responses[200] = "List of manufacturer names";
            s.Responses[503] = "External service unavailable";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var isAvailable = await externalIrCodeService.IsAvailable();
        if (!isAvailable)
        {
            await SendAsync([], 503, ct);
            return;
        }

        var manufacturers = await externalIrCodeService.GetAvailableManufacturers();
        await SendOkAsync(manufacturers, ct);
    }
}