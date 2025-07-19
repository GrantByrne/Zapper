using FastEndpoints;
using Zapper.Client.Devices;
using Zapper.Core.Models;
using Zapper.Device.Tizen;
using Zapper.Services;

namespace Zapper.API.Endpoints.Devices.Tizen;

public class PairTizenDeviceEndpoint(IDeviceService deviceService, ITizenDiscovery tizenDiscovery) : Endpoint<PairTizenDeviceRequest, PairTizenDeviceResponse>
{
    public override void Configure()
    {
        Post("/api/devices/{id}/pair/tizen");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Pair with a Samsung Tizen device";
            s.Description = "Initiate pairing process with a Samsung Tizen TV. User must accept the pairing request on the TV screen.";
        });
    }

    public override async Task HandleAsync(PairTizenDeviceRequest req, CancellationToken ct)
    {
        var device = await deviceService.GetDevice(req.DeviceId);
        if (device == null)
        {
            await SendAsync(new PairTizenDeviceResponse
            {
                Success = false,
                Message = "Device not found"
            }, 404, ct);
            return;
        }

        if (device.ConnectionType != ConnectionType.Tizen)
        {
            await SendAsync(new PairTizenDeviceResponse
            {
                Success = false,
                Message = "Device is not a Samsung Tizen device"
            }, 400, ct);
            return;
        }

        var success = await tizenDiscovery.PairWithDevice(device, req.PinCode, ct);
        if (success)
        {
            await deviceService.UpdateDevice(device.Id, device);

            await SendOkAsync(new PairTizenDeviceResponse
            {
                Success = true,
                Message = "Pairing successful",
                AuthToken = device.AuthenticationToken
            }, ct);
        }
        else
        {
            await SendAsync(new PairTizenDeviceResponse
            {
                Success = false,
                Message = "Pairing failed. Ensure the TV is on and you accept the pairing request on the TV screen."
            }, 400, ct);
        }
    }
}