using FastEndpoints;
using Zapper.Services;

namespace Zapper.API.Endpoints.IRCodes;

public class ImportExternalCodeSetEndpoint(IExternalIrCodeService externalIrCodeService, IIrCodeService irCodeService) : Endpoint<ImportExternalCodeSetRequest, ImportExternalCodeSetResponse>
{
    public override void Configure()
    {
        Post("/api/ir-codes/external/import");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Import IR code set from external database";
            s.Description = "Imports a complete IR code set from the IRDB external database into the local database. This is useful for quickly adding support for new devices without manually programming IR codes.";
            s.ExampleRequest = new ImportExternalCodeSetRequest
            {
                Manufacturer = "Samsung",
                DeviceType = "TV",
                Device = "UN55MU8000",
                Subdevice = ""
            };
            s.Responses[200] = "Import successful";
            s.Responses[404] = "Code set not found in external database";
            s.Responses[409] = "Code set already exists locally";
            s.Responses[500] = "Internal server error";
            s.Responses[503] = "External service unavailable";
        });
        Tags("IR Codes", "External");
    }

    public override async Task HandleAsync(ImportExternalCodeSetRequest req, CancellationToken ct)
    {
        var isAvailable = await externalIrCodeService.IsAvailable();
        if (!isAvailable)
        {
            await SendAsync(new ImportExternalCodeSetResponse
            {
                Success = false,
                Message = "External IR database is not available"
            }, 503, ct);
            return;
        }

        var existingCodeSet = await irCodeService.GetCodeSet(req.Manufacturer, req.Device,
            MapDeviceType(req.DeviceType));

        if (existingCodeSet != null)
        {
            await SendAsync(new ImportExternalCodeSetResponse
            {
                Success = false,
                Message = "Code set already exists in local database",
                CodeSetId = existingCodeSet.Id
            }, 409, ct);
            return;
        }

        var externalCodeSet = await externalIrCodeService.GetCodeSet(req.Manufacturer, req.DeviceType, req.Device, req.Subdevice);

        if (externalCodeSet == null)
        {
            await SendAsync(new ImportExternalCodeSetResponse
            {
                Success = false,
                Message = "Code set not found in external database"
            }, 404, ct);
            return;
        }

        var createdCodeSet = await irCodeService.CreateCodeSet(externalCodeSet);

        await SendOkAsync(new ImportExternalCodeSetResponse
        {
            Success = true,
            Message = $"Successfully imported {externalCodeSet.Codes.Count} IR codes",
            CodeSetId = createdCodeSet.Id
        }, ct);
    }

    private static Core.Models.DeviceType MapDeviceType(string deviceType)
    {
        return deviceType.ToLowerInvariant() switch
        {
            "tv" => Core.Models.DeviceType.Television,
            "dvd" => Core.Models.DeviceType.DvdPlayer,
            "stb" or "set-top-box" => Core.Models.DeviceType.CableBox,
            "ac" or "air-conditioner" => Core.Models.DeviceType.Receiver,
            "fan" => Core.Models.DeviceType.Receiver,
            "audio" or "stereo" => Core.Models.DeviceType.Receiver,
            _ => Core.Models.DeviceType.Television
        };
    }
}