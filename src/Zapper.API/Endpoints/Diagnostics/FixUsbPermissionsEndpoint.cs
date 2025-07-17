using FastEndpoints;
using Zapper.Core.Models;
using Zapper.Services;

namespace Zapper.API.Endpoints.Diagnostics;

public class FixUsbPermissionsEndpoint(ISystemDiagnosticsService diagnosticsService) : Endpoint<FixUsbPermissionsRequest, UsbPermissionFixResult>
{
    public override void Configure()
    {
        Post("/api/diagnostics/fix-usb-permissions");
        AllowAnonymous();
    }

    public override async Task HandleAsync(FixUsbPermissionsRequest req, CancellationToken ct)
    {
        var result = await diagnosticsService.FixUsbPermissionsAsync(req.Password);

        if (result.Success)
        {
            await SendOkAsync(result, ct);
        }
        else
        {
            await SendAsync(result, 500, ct);
        }
    }
}