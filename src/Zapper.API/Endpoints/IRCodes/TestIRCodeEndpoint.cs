using FastEndpoints;
using Zapper.Core.Interfaces;
using Zapper.Services;

namespace Zapper.API.Endpoints.IRCodes;

public class TestIrCodeEndpoint(IIrCodeService irCodeService, IDeviceController deviceController) : Endpoint<TestIrCodeRequest, TestIrCodeResponse>
{
    public override void Configure()
    {
        Post("/api/ir-codes/test");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Test an IR code by sending it through the transmitter";
            s.Description = "Sends a specific IR command to test if the code works with the target device. This is useful for verifying IR codes before assigning them to devices.";
            s.ExampleRequest = new TestIrCodeRequest
            {
                CodeSetId = 1,
                CommandName = "Power"
            };
            s.Responses[200] = "Command sent successfully";
            s.Responses[404] = "Code set or command not found";
            s.Responses[500] = "Internal server error";
            s.Responses[503] = "IR transmitter not available";
        });
        Tags("IR Codes");
    }

    public override async Task HandleAsync(TestIrCodeRequest req, CancellationToken ct)
    {
        var codeSet = await irCodeService.GetCodeSet(req.CodeSetId);
        if (codeSet == null)
        {
            await SendAsync(new TestIrCodeResponse
            {
                Success = false,
                Message = "Code set not found"
            }, 404, ct);
            return;
        }

        var irCode = codeSet.Codes.FirstOrDefault(c =>
            c.CommandName.Equals(req.CommandName, StringComparison.OrdinalIgnoreCase));

        if (irCode == null)
        {
            await SendAsync(new TestIrCodeResponse
            {
                Success = false,
                Message = $"Command '{req.CommandName}' not found in code set"
            }, 404, ct);
            return;
        }

        try
        {
            var deviceCommand = new Core.Models.DeviceCommand
            {
                Name = irCode.CommandName,
                Type = MapCommandType(irCode.CommandName),
                IrCode = irCode.HexCode
            };

            // Create a temporary device for testing
            var testDevice = new Core.Models.Device
            {
                Id = 0,
                ConnectionType = Core.Models.ConnectionType.InfraredIr,
                Name = "Test Device"
            };

            var result = await deviceController.SendCommand(testDevice, deviceCommand);

            if (result)
            {
                await SendOkAsync(new TestIrCodeResponse
                {
                    Success = true,
                    Message = $"Successfully sent {req.CommandName} command"
                }, ct);
            }
            else
            {
                await SendAsync(new TestIrCodeResponse
                {
                    Success = false,
                    Message = "Failed to send command"
                }, 503, ct);
            }
        }
        catch (Exception ex)
        {
            await SendAsync(new TestIrCodeResponse
            {
                Success = false,
                Message = $"Error sending command: {ex.Message}"
            }, 500, ct);
        }
    }

    private static Core.Models.CommandType MapCommandType(string commandName)
    {
        return commandName.ToLowerInvariant() switch
        {
            "power" => Core.Models.CommandType.Power,
            "volumeup" => Core.Models.CommandType.VolumeUp,
            "volumedown" => Core.Models.CommandType.VolumeDown,
            "mute" => Core.Models.CommandType.Mute,
            "channelup" => Core.Models.CommandType.ChannelUp,
            "channeldown" => Core.Models.CommandType.ChannelDown,
            "input" => Core.Models.CommandType.Input,
            "menu" => Core.Models.CommandType.Menu,
            "back" => Core.Models.CommandType.Back,
            "home" => Core.Models.CommandType.Home,
            "ok" => Core.Models.CommandType.Ok,
            "directionalup" or "up" => Core.Models.CommandType.DirectionalUp,
            "directionaldown" or "down" => Core.Models.CommandType.DirectionalDown,
            "directionalleft" or "left" => Core.Models.CommandType.DirectionalLeft,
            "directionalright" or "right" => Core.Models.CommandType.DirectionalRight,
            "play" or "pause" or "playpause" => Core.Models.CommandType.PlayPause,
            "stop" => Core.Models.CommandType.Stop,
            "fastforward" or "forward" => Core.Models.CommandType.FastForward,
            "rewind" => Core.Models.CommandType.Rewind,
            "record" => Core.Models.CommandType.Record,
            _ => Core.Models.CommandType.Custom
        };
    }
}