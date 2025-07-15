using FastEndpoints;
using Zapper.Services;

namespace Zapper.API.Endpoints.IRCodes;

public class LearnIrCommandEndpoint(IIrLearningService irLearningService) : Endpoint<LearnIrCommandRequest, LearnIrCommandResponse>
{
    public override void Configure()
    {
        Post("/api/ir-codes/learn");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Learn an IR command from a remote control";
            s.Description = "Captures an IR signal from a remote control and returns the decoded command. Point your remote at the IR receiver and press the button when ready.";
            s.ExampleRequest = new LearnIrCommandRequest
            {
                CommandName = "Power",
                TimeoutSeconds = 30
            };
            s.Responses[200] = "Command learned successfully";
            s.Responses[408] = "Timeout - no IR signal received";
            s.Responses[500] = "Internal server error";
            s.Responses[503] = "IR receiver not available";
        });
        Tags("IR Codes");
    }

    public override async Task HandleAsync(LearnIrCommandRequest req, CancellationToken ct)
    {
        if (!irLearningService.IsReceiverAvailable)
        {
            await SendAsync(new LearnIrCommandResponse
            {
                Success = false,
                Message = "IR receiver not available. Please ensure the IR receiver is properly connected."
            }, 503, ct);
            return;
        }

        if (string.IsNullOrWhiteSpace(req.CommandName))
        {
            await SendAsync(new LearnIrCommandResponse
            {
                Success = false,
                Message = "Command name is required"
            }, 400, ct);
            return;
        }

        var timeout = TimeSpan.FromSeconds(Math.Max(5, Math.Min(120, req.TimeoutSeconds)));

        try
        {
            var learnedCode = await irLearningService.LearnCommandAsync(req.CommandName, timeout, ct);

            if (learnedCode == null)
            {
                await SendAsync(new LearnIrCommandResponse
                {
                    Success = false,
                    Message = $"No IR signal received within {timeout.TotalSeconds} seconds. Please ensure your remote is pointed at the IR receiver and try again."
                }, 408, ct);
                return;
            }

            await SendOkAsync(new LearnIrCommandResponse
            {
                Success = true,
                Message = $"Successfully learned {req.CommandName} command using {learnedCode.Protocol} protocol",
                LearnedCode = learnedCode
            }, ct);
        }
        catch (OperationCanceledException)
        {
            await SendAsync(new LearnIrCommandResponse
            {
                Success = false,
                Message = "Learning operation was cancelled"
            }, 408, ct);
        }
        catch (Exception ex)
        {
            await SendAsync(new LearnIrCommandResponse
            {
                Success = false,
                Message = $"Error learning command: {ex.Message}"
            }, 500, ct);
        }
    }
}