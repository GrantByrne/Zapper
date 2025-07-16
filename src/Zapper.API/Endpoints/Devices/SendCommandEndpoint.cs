using FastEndpoints;
using Zapper.Services;
using Zapper.Client.Devices;

namespace Zapper.API.Endpoints.Devices;

public class SendCommandEndpoint(IDeviceService deviceService) : Endpoint<SendCommandApiRequest, SendCommandResponse>
{
    public override void Configure()
    {
        Post("/api/devices/{id}/commands");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Send command to device";
            s.Description = "Execute a specific command on a device. Commands can be IR codes, network commands, or Bluetooth commands depending on the device type.";
            s.ExampleRequest = new SendCommandApiRequest
            {
                Id = 1,
                Command = "power",
                MouseDeltaX = null,
                MouseDeltaY = null,
                KeyboardText = null
            };
            s.Responses[200] = "Command sent successfully";
            s.Responses[400] = "Failed to send command - invalid device ID or command";
            s.Responses[404] = "Device not found";
            s.Responses[500] = "Internal server error";
        });
        Tags("Devices");
    }

    public override async Task HandleAsync(SendCommandApiRequest req, CancellationToken ct)
    {
        var sendCommandRequest = new SendCommandRequest
        {
            Command = req.Command,
            MouseDeltaX = req.MouseDeltaX,
            MouseDeltaY = req.MouseDeltaY,
            KeyboardText = req.KeyboardText,
            Parameters = req.Parameters
        };

        var success = await deviceService.SendCommand(req.Id, sendCommandRequest, ct);
        if (!success)
        {
            await SendAsync(new SendCommandResponse
            {
                Message = $"Failed to send command '{req.Command}' to device {req.Id}"
            }, 400, ct);
            return;
        }

        await SendOkAsync(new SendCommandResponse
        {
            Message = $"Command '{req.Command}' sent successfully"
        }, ct);
    }
}