using FastEndpoints;
using Zapper.Client.System;
using Zapper.Core.Models;
using Zapper.Services;

namespace Zapper.API.Endpoints.System;

public class TestGpioPinEndpoint(IIrTroubleshootingService troubleshootingService) : Endpoint<TestGpioPinRequest, GpioTestResult>
{
    public override void Configure()
    {
        Post("/api/system/test-gpio-pin");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Test GPIO pin functionality";
            s.Description = "Tests a specific GPIO pin to verify it can be accessed and controlled.";
            s.ExampleRequest = new TestGpioPinRequest { Pin = 18, IsOutput = true };
            s.Responses[200] = "Test completed successfully";
            s.Responses[400] = "Invalid pin number";
            s.Responses[500] = "Internal server error";
        });
        Tags("System", "Troubleshooting");
    }

    public override async Task HandleAsync(TestGpioPinRequest req, CancellationToken ct)
    {
        if (req.Pin < 0 || req.Pin > 40)
        {
            await SendAsync(new GpioTestResult
            {
                IsAvailable = false,
                CanAccess = false,
                Message = "Invalid GPIO pin number. Must be between 0 and 40.",
                Pin = req.Pin
            }, 400, ct);
            return;
        }

        var result = await troubleshootingService.TestGpioPinAsync(req.Pin, req.IsOutput);
        await SendOkAsync(result, ct);
    }
}