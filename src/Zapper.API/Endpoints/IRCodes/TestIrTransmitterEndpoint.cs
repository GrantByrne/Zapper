using FastEndpoints;
using Zapper.Core.Models;
using Zapper.Services;

namespace Zapper.API.Endpoints.IRCodes;

public class TestIrTransmitterEndpoint(IIrTroubleshootingService troubleshootingService) : EndpointWithoutRequest<IrHardwareTestResult>
{
    public override void Configure()
    {
        Post("/api/ir-codes/test-transmitter");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Test IR transmitter hardware";
            s.Description = "Tests the IR transmitter by sending a test signal to verify hardware functionality.";
            s.Responses[200] = "Test completed successfully";
            s.Responses[500] = "Internal server error";
        });
        Tags("IR Codes", "Troubleshooting");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await troubleshootingService.TestIrTransmitterAsync();
        await SendOkAsync(result, ct);
    }
}