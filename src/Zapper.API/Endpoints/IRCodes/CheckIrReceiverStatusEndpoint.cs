using FastEndpoints;
using Zapper.Services;

namespace Zapper.API.Endpoints.IRCodes;

public class CheckIrReceiverStatusResponse
{
    public bool IsAvailable { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class CheckIrReceiverStatusEndpoint(IIrLearningService irLearningService) : EndpointWithoutRequest<CheckIrReceiverStatusResponse>
{
    public override void Configure()
    {
        Get("/api/ir-codes/receiver-status");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Check if IR receiver is available for learning";
            s.Description = "Returns the status of the IR receiver hardware to determine if learning functionality is available.";
            s.Responses[200] = "Status retrieved successfully";
        });
        Tags("IR Codes");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var isAvailable = irLearningService.IsReceiverAvailable;
        var message = isAvailable
            ? "IR receiver is available and ready for learning"
            : "IR receiver is not available. Please check hardware connection.";

        await SendOkAsync(new CheckIrReceiverStatusResponse
        {
            IsAvailable = isAvailable,
            Message = message
        }, ct);
    }
}