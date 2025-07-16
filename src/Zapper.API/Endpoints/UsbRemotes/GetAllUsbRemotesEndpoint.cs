using FastEndpoints;
using Zapper.Core.Models;
using Zapper.Services;

namespace Zapper.API.Endpoints.UsbRemotes;

public class GetAllUsbRemotesEndpoint(IUsbRemoteService usbRemoteService) : EndpointWithoutRequest<IEnumerable<UsbRemote>>
{
    public override void Configure()
    {
        Get("/api/usb-remotes");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get all USB remotes";
            s.Description = "Retrieve a list of all registered USB remotes in the system";
            s.Responses[200] = "List of USB remotes retrieved successfully";
            s.Responses[500] = "Internal server error";
        });
        Tags("USB Remotes");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var remotes = await usbRemoteService.GetAllRemotesAsync();
        await SendOkAsync(remotes, ct);
    }
}