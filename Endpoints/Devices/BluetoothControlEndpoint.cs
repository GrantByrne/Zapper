using FastEndpoints;
using ZapperHub.Hardware;

namespace ZapperHub.Endpoints.Devices;

public class BluetoothControlRequest
{
    public string Action { get; set; } = string.Empty;
    public string? DeviceId { get; set; }
    public string? KeyCode { get; set; }
    public string? Text { get; set; }
    public int? MouseX { get; set; }
    public int? MouseY { get; set; }
    public bool? LeftClick { get; set; }
    public bool? RightClick { get; set; }
}

public class BluetoothControlResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class BluetoothControlEndpoint : Endpoint<BluetoothControlRequest, BluetoothControlResponse>
{
    public IBluetoothHIDController BluetoothController { get; set; } = null!;

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
                "disconnect" => await BluetoothController.DisconnectAsync(ct),
                "start_advertising" => await BluetoothController.StartAdvertisingAsync(ct),
                "stop_advertising" => await BluetoothController.StopAdvertisingAsync(ct),
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
        return await BluetoothController.ConnectToDeviceAsync(req.DeviceId, ct);
    }

    private async Task<bool> HandleSendKey(BluetoothControlRequest req, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(req.KeyCode) || !Enum.TryParse<HIDKeyCode>(req.KeyCode, true, out var keyCode))
        {
            return false;
        }
        return await BluetoothController.SendKeyEventAsync(keyCode, true, ct);
    }

    private async Task<bool> HandleSendMouse(BluetoothControlRequest req, CancellationToken ct)
    {
        var deltaX = req.MouseX ?? 0;
        var deltaY = req.MouseY ?? 0;
        var leftClick = req.LeftClick ?? false;
        var rightClick = req.RightClick ?? false;
        
        return await BluetoothController.SendMouseEventAsync(deltaX, deltaY, leftClick, rightClick, ct);
    }

    private async Task<bool> HandleSendText(BluetoothControlRequest req, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(req.Text))
        {
            return false;
        }
        return await BluetoothController.SendKeyboardTextAsync(req.Text, ct);
    }
}