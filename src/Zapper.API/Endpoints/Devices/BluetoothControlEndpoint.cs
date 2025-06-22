using FastEndpoints;
using Zapper.API.Models.Requests;
using Zapper.API.Models.Responses;
using Zapper.Integrations;

namespace Zapper.Endpoints.Devices;

public class BluetoothControlEndpoint(IBluetoothHIDController bluetoothController) : Endpoint<BluetoothControlRequest, BluetoothControlResponse>
{

    public override void Configure()
    {
        Post("/api/devices/bluetooth/control");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Control Bluetooth HID device";
            s.Description = "Send commands to a connected Bluetooth device (key events, mouse events, text input)";
        });
    }

    public override async Task HandleAsync(BluetoothControlRequest req, CancellationToken ct)
    {
        try
        {
            bool success = req.Action.ToLower() switch
            {
                "connect" => await HandleConnect(req, ct),
                "disconnect" => await bluetoothController.DisconnectAsync(ct),
                "start_advertising" => await bluetoothController.StartAdvertisingAsync(ct),
                "stop_advertising" => await bluetoothController.StopAdvertisingAsync(ct),
                "send_key" => await HandleSendKey(req, ct),
                "send_mouse" => await HandleSendMouse(req, ct),
                "send_text" => await HandleSendText(req, ct),
                _ => false
            };

            if (success)
            {
                await SendOkAsync(new BluetoothControlResponse
                {
                    Success = true,
                    Message = $"Action '{req.Action}' completed successfully"
                }, ct);
            }
            else
            {
                await SendAsync(new BluetoothControlResponse
                {
                    Success = false,
                    Message = $"Action '{req.Action}' failed"
                }, 400, ct);
            }
        }
        catch (Exception ex)
        {
            await SendAsync(new BluetoothControlResponse
            {
                Success = false,
                Message = $"Error executing action '{req.Action}': {ex.Message}"
            }, 500, ct);
        }
    }

    private async Task<bool> HandleConnect(BluetoothControlRequest req, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(req.DeviceId))
        {
            return false;
        }
        return await bluetoothController.ConnectToDeviceAsync(req.DeviceId, ct);
    }

    private async Task<bool> HandleSendKey(BluetoothControlRequest req, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(req.KeyCode) || !Enum.TryParse<HIDKeyCode>(req.KeyCode, true, out var keyCode))
        {
            return false;
        }
        return await bluetoothController.SendKeyEventAsync(keyCode, true, ct);
    }

    private async Task<bool> HandleSendMouse(BluetoothControlRequest req, CancellationToken ct)
    {
        var deltaX = req.MouseX ?? 0;
        var deltaY = req.MouseY ?? 0;
        var leftClick = req.LeftClick ?? false;
        var rightClick = req.RightClick ?? false;
        
        return await bluetoothController.SendMouseEventAsync(deltaX, deltaY, leftClick, rightClick, ct);
    }

    private async Task<bool> HandleSendText(BluetoothControlRequest req, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(req.Text))
        {
            return false;
        }
        return await bluetoothController.SendKeyboardTextAsync(req.Text, ct);
    }
}