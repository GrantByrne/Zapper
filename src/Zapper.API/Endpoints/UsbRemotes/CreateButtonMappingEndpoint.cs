using FastEndpoints;
using Zapper.Client.UsbRemotes;
using Zapper.Core.Models;
using Zapper.Services;

namespace Zapper.API.Endpoints.UsbRemotes;

public class CreateButtonMappingEndpoint(IUsbRemoteService usbRemoteService) : Endpoint<CreateButtonMappingRequest, UsbRemoteButtonMapping>
{
    public override void Configure()
    {
        Post("/api/usb-remotes/button-mappings");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Create button mapping";
            s.Description = "Create a new button mapping for a USB remote button";
            s.Responses[200] = "Button mapping created successfully";
            s.Responses[400] = "Invalid request";
            s.Responses[500] = "Internal server error";
        });
        Tags("USB Remotes");
    }

    public override async Task HandleAsync(CreateButtonMappingRequest req, CancellationToken ct)
    {
        try
        {
            var mapping = await usbRemoteService.CreateButtonMappingAsync(
                req.ButtonId,
                req.DeviceId,
                req.DeviceCommandId,
                req.EventType);

            await SendOkAsync(mapping, ct);
        }
        catch (Exception)
        {
            await SendErrorsAsync(500, ct);
        }
    }
}