using FastEndpoints;
using Zapper.API.Models.Requests;
using Zapper.API.Models.Responses;
using Zapper.Core.Models;
using Zapper.Device.WebOS;
using Zapper.Services;

namespace Zapper.API.Endpoints.Devices;

public class PairWebOsDeviceEndpoint(IDeviceService deviceService, IWebOsDiscovery webOsDiscovery) : Endpoint<PairWebOsDeviceRequest, PairWebOsDeviceResponse>
{

    public override void Configure()
    {
        Post("/api/devices/{id}/pair/webos");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Pair with a WebOS device";
            s.Description = "Initiate pairing process with a WebOS TV. User must accept the pairing request on the TV screen.";
        });
    }

    public override async Task HandleAsync(PairWebOsDeviceRequest req, CancellationToken ct)
    {
        var device = await deviceService.GetDevice(req.DeviceId);
        if (device == null)
        {
            await SendAsync(new PairWebOsDeviceResponse
            {
                Success = false,
                Message = "Device not found"
            }, 404, ct);
            return;
        }

        if (device.ConnectionType != ConnectionType.WebOs)
        {
            await SendAsync(new PairWebOsDeviceResponse
            {
                Success = false,
                Message = "Device is not a WebOS device"
            }, 400, ct);
            return;
        }

        var success = await webOsDiscovery.PairWithDevice(device, ct);
        if (success)
        {
            await deviceService.UpdateDevice(device.Id, device);

            await SendOkAsync(new PairWebOsDeviceResponse
            {
                Success = true,
                Message = "Pairing successful",
                ClientKey = device.AuthenticationToken
            }, ct);
        }
        else
        {
            await SendAsync(new PairWebOsDeviceResponse
            {
                Success = false,
                Message = "Pairing failed. Ensure the TV is on and you accept the pairing request on the TV screen."
            }, 400, ct);
        }
    }
}