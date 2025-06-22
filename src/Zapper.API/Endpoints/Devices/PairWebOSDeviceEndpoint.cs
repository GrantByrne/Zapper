using FastEndpoints;
using Zapper.API.Models.Requests;
using Zapper.API.Models.Responses;
using Zapper.Core.Models;
using Zapper.Integrations;
using Zapper.Services;

namespace Zapper.Endpoints.Devices;

public class PairWebOSDeviceEndpoint(IDeviceService deviceService, IWebOSDiscovery webOSDiscovery) : Endpoint<PairWebOSDeviceRequest, PairWebOSDeviceResponse>
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

    public override async Task HandleAsync(PairWebOSDeviceRequest req, CancellationToken ct)
    {
        var device = await deviceService.GetDeviceAsync(req.DeviceId);
        if (device == null)
        {
            await SendAsync(new PairWebOSDeviceResponse
            {
                Success = false,
                Message = "Device not found"
            }, 404, ct);
            return;
        }

        if (device.ConnectionType != ConnectionType.WebOS)
        {
            await SendAsync(new PairWebOSDeviceResponse
            {
                Success = false,
                Message = "Device is not a WebOS device"
            }, 400, ct);
            return;
        }

        var success = await webOSDiscovery.PairWithDeviceAsync(device, ct);
        if (success)
        {
            // Update the device with the new authentication token
            await deviceService.UpdateDeviceAsync(device.Id, device);

            await SendOkAsync(new PairWebOSDeviceResponse
            {
                Success = true,
                Message = "Pairing successful",
                ClientKey = device.AuthenticationToken
            }, ct);
        }
        else
        {
            await SendAsync(new PairWebOSDeviceResponse
            {
                Success = false,
                Message = "Pairing failed. Ensure the TV is on and you accept the pairing request on the TV screen."
            }, 400, ct);
        }
    }
}