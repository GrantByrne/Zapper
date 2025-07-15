using FastEndpoints;
using Zapper.Services;

namespace Zapper.API.Endpoints.Devices;

public class SendCommandEndpoint(IDeviceService deviceService) : Endpoint<SendCommandRequest, SendCommandResponse>
{
    public override void Configure()
    {
        Post("/api/devices/{id}/commands/{commandName}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Send command to device";
            s.Description = "Execute a specific command on a device. Commands can be IR codes, network commands, or Bluetooth commands depending on the device type.";
            s.ExampleRequest = new SendCommandRequest { Id = 1, CommandName = "Power" };
            s.Responses[200] = "Command sent successfully";
            s.Responses[400] = "Failed to send command - invalid device ID or command";
            s.Responses[404] = "Device not found";
            s.Responses[500] = "Internal server error";
        });
        Tags("Devices");
    }

    public override async Task HandleAsync(SendCommandRequest req, CancellationToken ct)
    {
        var success = await deviceService.SendCommand(req.Id, req.CommandName, ct);
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