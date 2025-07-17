using FastEndpoints;
using Zapper.Core.Models;
using Zapper.Services;

namespace Zapper.API.Endpoints.Diagnostics;

public class CheckUsbPermissionsEndpoint(ISystemDiagnosticsService diagnosticsService) : EndpointWithoutRequest<UsbPermissionStatus>
{
    public override void Configure()
    {
        Get("/api/diagnostics/usb-permissions");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var status = await diagnosticsService.CheckUsbPermissionsAsync();
        await SendOkAsync(status, ct);
    }
}