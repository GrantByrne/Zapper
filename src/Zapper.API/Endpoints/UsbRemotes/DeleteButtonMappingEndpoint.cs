using FastEndpoints;
using Zapper.Services;

namespace Zapper.API.Endpoints.UsbRemotes;

public class DeleteButtonMappingRequest
{
    public int Id { get; set; }
}

public class DeleteButtonMappingResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
}

public class DeleteButtonMappingEndpoint(IUsbRemoteService usbRemoteService) : Endpoint<DeleteButtonMappingRequest, DeleteButtonMappingResponse>
{
    public override void Configure()
    {
        Delete("/api/usb-remotes/button-mappings/{id}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Delete button mapping";
            s.Description = "Delete a button mapping from a USB remote";
            s.Responses[200] = "Button mapping deleted successfully";
            s.Responses[404] = "Button mapping not found";
            s.Responses[500] = "Internal server error";
        });
        Tags("USB Remotes");
    }

    public override async Task HandleAsync(DeleteButtonMappingRequest req, CancellationToken ct)
    {
        try
        {
            await usbRemoteService.DeleteButtonMappingAsync(req.Id);

            await SendOkAsync(new DeleteButtonMappingResponse
            {
                Success = true,
                Message = "Button mapping deleted successfully"
            }, ct);
        }
        catch (Exception)
        {
            await SendErrorsAsync(500, ct);
        }
    }
}