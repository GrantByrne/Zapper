using FastEndpoints;
using Zapper.Contracts.IRCodes;
using Zapper.Core.Models;
using Zapper.Services;

namespace Zapper.API.Endpoints.IRCodes;

public class TestIrReceiverEndpoint(IIrTroubleshootingService troubleshootingService) : Endpoint<TestIrReceiverRequest, IrHardwareTestResult>
{
    public override void Configure()
    {
        Post("/api/ir-codes/test-receiver");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Test IR receiver hardware";
            s.Description = "Tests the IR receiver by attempting to capture an IR signal within the specified timeout.";
            s.ExampleRequest = new TestIrReceiverRequest { TimeoutSeconds = 10 };
            s.Responses[200] = "Test completed successfully";
            s.Responses[500] = "Internal server error";
        });
        Tags("IR Codes", "Troubleshooting");
    }

    public override async Task HandleAsync(TestIrReceiverRequest req, CancellationToken ct)
    {
        var timeout = TimeSpan.FromSeconds(Math.Max(5, Math.Min(60, req.TimeoutSeconds)));
        var result = await troubleshootingService.TestIrReceiverAsync(timeout);
        await SendOkAsync(result, ct);
    }
}