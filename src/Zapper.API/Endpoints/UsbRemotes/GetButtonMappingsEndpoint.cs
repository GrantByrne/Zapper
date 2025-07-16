using FastEndpoints;
using Zapper.Client.UsbRemotes;
using Zapper.Core.Models;
using Zapper.Services;

namespace Zapper.API.Endpoints.UsbRemotes;

public class GetButtonMappingsEndpoint(IUsbRemoteService usbRemoteService) : Endpoint<GetButtonMappingsRequest, IEnumerable<UsbRemoteButton>>
{
    public override void Configure()
    {
        Get("/api/usb-remotes/{remoteId}/buttons");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get button mappings for a USB remote";
            s.Description = "Retrieve all button mappings for a specific USB remote";
            s.Responses[200] = "Button mappings retrieved successfully";
            s.Responses[404] = "USB remote not found";
            s.Responses[500] = "Internal server error";
        });
        Tags("USB Remotes");
    }

    public override async Task HandleAsync(GetButtonMappingsRequest req, CancellationToken ct)
    {
        var buttons = await usbRemoteService.GetRemoteButtonsAsync(req.RemoteId);
        await SendOkAsync(buttons, ct);
    }
}