using FastEndpoints;
using Zapper.Services;

namespace Zapper.API.Endpoints.IRCodes;

public class SearchExternalDevicesEndpoint(IExternalIrCodeService externalIrCodeService) : Endpoint<SearchExternalDevicesRequest, SearchExternalDevicesResponse>
{
    public override void Configure()
    {
        Get("/api/ir-codes/external/devices/search");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Search devices in external IR database";
            s.Description = "Search for devices by manufacturer and/or device type in the IRDB external database";
            s.Responses[200] = "List of matching devices";
            s.Responses[503] = "External service unavailable";
        });
    }

    public override async Task HandleAsync(SearchExternalDevicesRequest req, CancellationToken ct)
    {
        var isAvailable = await externalIrCodeService.IsAvailable();
        if (!isAvailable)
        {
            await SendAsync(new SearchExternalDevicesResponse(), 503, ct);
            return;
        }

        var devices = await externalIrCodeService.SearchDevices(req.Manufacturer, req.DeviceType);

        var response = new SearchExternalDevicesResponse
        {
            Devices = devices.Select(d => new ExternalDeviceInfo
            {
                Manufacturer = d.Manufacturer,
                DeviceType = d.DeviceType,
                Device = d.Device,
                Subdevice = d.Subdevice
            })
        };

        await SendOkAsync(response, ct);
    }
}