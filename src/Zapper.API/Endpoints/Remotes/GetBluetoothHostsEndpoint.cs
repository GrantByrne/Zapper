using FastEndpoints;
using Zapper.Client.Remotes;
using Zapper.Services;

namespace Zapper.API.Endpoints.Remotes;

public class GetBluetoothHostsEndpoint(
    IBluetoothRemoteService bluetoothRemoteService) : EndpointWithoutRequest<BluetoothHostsResponse>
{
    public override void Configure()
    {
        Get("/api/remotes/bluetooth/hosts");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get paired Bluetooth hosts";
            s.Description = "Returns a list of devices that have paired with the Pi as a remote";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            var hosts = await bluetoothRemoteService.GetPairedHostsAsync(ct);

            await SendOkAsync(new BluetoothHostsResponse
            {
                Hosts = hosts.Select(h => new BluetoothHostInfo
                {
                    Address = h.Address,
                    Name = h.Name,
                    LastConnected = h.LastConnected,
                    IsPaired = h.IsPaired
                }).ToList()
            }, ct);
        }
        catch (Exception)
        {
            await SendErrorsAsync(500, ct);
        }
    }
}