using FastEndpoints;
using Zapper.Client.UsbRemotes;
using Zapper.Core.Models;
using Zapper.Device.USB;
using Zapper.Services;

namespace Zapper.API.Endpoints.UsbRemotes;

public class LearnButtonEndpoint(
    IUsbRemoteService usbRemoteService,
    IUsbRemoteHandler usbRemoteHandler) : Endpoint<LearnButtonRequest, LearnButtonResponse>
{
    public override void Configure()
    {
        Post("/api/usb-remotes/{remoteId}/learn-button");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Learn button from USB remote";
            s.Description = "Enter learning mode to capture a button press from a USB remote";
            s.Responses[200] = "Button learned successfully";
            s.Responses[404] = "USB remote not found";
            s.Responses[408] = "Timeout waiting for button press";
            s.Responses[500] = "Internal server error";
        });
        Tags("USB Remotes");
    }

    public override async Task HandleAsync(LearnButtonRequest req, CancellationToken ct)
    {
        var remote = await usbRemoteService.GetRemoteByIdAsync(req.RemoteId);
        if (remote == null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        var tcs = new TaskCompletionSource<RemoteButtonEventArgs>();
        var timeout = TimeSpan.FromSeconds(req.TimeoutSeconds);

        void OnButtonPressed(object? sender, RemoteButtonEventArgs e)
        {
            if (e.DeviceId == remote.DeviceId && e.EventType == ButtonEventType.KeyPress)
            {
                tcs.TrySetResult(e);
            }
        }

        try
        {
            usbRemoteHandler.ButtonPressed += OnButtonPressed;

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(timeout);

            var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(timeout, cts.Token));

            if (completedTask == tcs.Task)
            {
                var buttonEvent = await tcs.Task;

                // Register the button if it's new
                var button = await usbRemoteService.RegisterButtonAsync(
                    req.RemoteId,
                    (byte)buttonEvent.KeyCode,
                    buttonEvent.ButtonName);

                await SendOkAsync(new LearnButtonResponse
                {
                    Success = true,
                    KeyCode = (byte)buttonEvent.KeyCode,
                    ButtonName = buttonEvent.ButtonName,
                    Message = $"Button '{buttonEvent.ButtonName}' learned successfully"
                }, ct);
            }
            else
            {
                await SendAsync(new LearnButtonResponse
                {
                    Success = false,
                    Message = "Timeout waiting for button press"
                }, 408, ct);
            }
        }
        finally
        {
            usbRemoteHandler.ButtonPressed -= OnButtonPressed;
        }
    }
}