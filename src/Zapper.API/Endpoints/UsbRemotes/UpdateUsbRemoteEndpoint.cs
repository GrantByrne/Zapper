using FastEndpoints;
using Zapper.Client.UsbRemotes;
using Zapper.Services;

namespace Zapper.API.Endpoints.UsbRemotes;

public class UpdateUsbRemoteEndpoint(IUsbRemoteService usbRemoteService) : Endpoint<UpdateUsbRemoteRequest, UpdateUsbRemoteResponse>
{
    public override void Configure()
    {
        Put("/api/usb-remotes/{id}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Update USB remote";
            s.Description = "Update configuration for a USB remote";
            s.Responses[200] = "USB remote updated successfully";
            s.Responses[400] = "Invalid request";
            s.Responses[404] = "USB remote not found";
            s.Responses[500] = "Internal server error";
        });
        Tags("USB Remotes");
    }

    public override async Task HandleAsync(UpdateUsbRemoteRequest req, CancellationToken ct)
    {
        try
        {
            await usbRemoteService.UpdateRemoteAsync(
                req.Id,
                req.Name,
                req.IsActive,
                req.InterceptSystemButtons,
                req.LongPressTimeoutMs);

            await SendOkAsync(new UpdateUsbRemoteResponse
            {
                Success = true,
                Message = "USB remote updated successfully"
            }, ct);
        }
        catch (ArgumentException)
        {
            await SendNotFoundAsync(ct);
        }
        catch (Exception)
        {
            await SendErrorsAsync(500, ct);
        }
    }
}