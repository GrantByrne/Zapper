using FastEndpoints;
using Zapper.Client.Devices;
using Zapper.Device.Bluetooth;

namespace Zapper.API.Endpoints.Devices.Bluetooth;

public class BluetoothControlEndpoint(IBluetoothHidController bluetoothController) : Endpoint<BluetoothControlRequest, BluetoothControlResponse>
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
            var success = req.Action.ToLower() switch
            {
                "connect" => await HandleConnect(req, ct),
                "disconnect" => await HandleDisconnect(req, ct),
                "send_key" => await HandleSendKey(req, ct),
                "send_text" => await HandleSendText(req, ct),
                "get_connected_devices" => await HandleGetConnectedDevices(req, ct),
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
        return await bluetoothController.Connect(req.DeviceId, ct);
    }

    private async Task<bool> HandleDisconnect(BluetoothControlRequest req, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(req.DeviceId))
        {
            return false;
        }
        return await bluetoothController.Disconnect(req.DeviceId, ct);
    }

    private async Task<bool> HandleSendKey(BluetoothControlRequest req, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(req.DeviceId) || string.IsNullOrEmpty(req.KeyCode) ||
            !Enum.TryParse<HidKeyCode>(req.KeyCode, true, out var keyCode))
        {
            return false;
        }
        return await bluetoothController.SendKey(req.DeviceId, keyCode, ct);
    }

    private async Task<bool> HandleSendText(BluetoothControlRequest req, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(req.DeviceId) || string.IsNullOrEmpty(req.Text))
        {
            return false;
        }
        return await bluetoothController.SendText(req.DeviceId, req.Text, ct);
    }

    private async Task<bool> HandleGetConnectedDevices(BluetoothControlRequest req, CancellationToken ct)
    {
        var devices = await bluetoothController.GetConnectedDevices(ct);
        return devices != null;
    }
}