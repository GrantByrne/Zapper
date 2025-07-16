using FastEndpoints;
using Zapper.Core.Models;
using Zapper.Services;

namespace Zapper.API.Endpoints.UsbRemotes;

public class GetUsbRemoteRequest
{
    public int Id { get; set; }
}

public class GetUsbRemoteEndpoint(IUsbRemoteService usbRemoteService) : Endpoint<GetUsbRemoteRequest, UsbRemote>
{
    public override void Configure()
    {
        Get("/api/usb-remotes/{id}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get USB remote by ID";
            s.Description = "Retrieve a specific USB remote by its ID";
            s.Responses[200] = "USB remote retrieved successfully";
            s.Responses[404] = "USB remote not found";
            s.Responses[500] = "Internal server error";
        });
        Tags("USB Remotes");
    }

    public override async Task HandleAsync(GetUsbRemoteRequest req, CancellationToken ct)
    {
        var remote = await usbRemoteService.GetRemoteByIdAsync(req.Id);
        if (remote == null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        await SendOkAsync(remote, ct);
    }
}