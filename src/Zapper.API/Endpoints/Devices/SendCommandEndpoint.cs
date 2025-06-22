using FastEndpoints;
using Zapper.Services;

namespace Zapper.Endpoints.Devices;

public class SendCommandRequest
{
    public int Id { get; set; }
    public string CommandName { get; set; } = string.Empty;
}

public class SendCommandResponse
{
    public string Message { get; set; } = string.Empty;
}

public class SendCommandEndpoint : Endpoint<SendCommandRequest, SendCommandResponse>
{
    public IDeviceService DeviceService { get; set; } = null!;

    public override void Configure()
    {
        Post("/api/devices/{id}/commands/{commandName}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Send command to device";
            s.Description = "Execute a specific command on a device";
        });
    }

    public override async Task HandleAsync(SendCommandRequest req, CancellationToken ct)
    {
        var success = await DeviceService.SendCommandAsync(req.Id, req.CommandName, ct);
        if (!success)
        {
            await SendAsync(new SendCommandResponse 
            { 
                Message = $"Failed to send command '{req.CommandName}' to device {req.Id}" 
            }, 400, ct);
            return;
        }

        await SendOkAsync(new SendCommandResponse 
        { 
            Message = $"Command '{req.CommandName}' sent successfully" 
        }, ct);
    }
}